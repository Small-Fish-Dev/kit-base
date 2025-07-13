namespace GameFish;

/// <summary>
/// Something an <see cref="global::GameFish.Agent"/> can control.
/// </summary>
[Icon( "person" )]
[EditorHandle( Icon = "person" )]
public abstract partial class Pawn : Component, ISimulate
{
	public const string FEATURE_PAWN = "♟️ Pawn";

	public enum AttemptResult
	{
		Failure,
		Request,
		Success
	}

	// public override string ToString()
	// => $"{GetType().ToSimpleString( includeNamespace: false )}|Agent:{Agent?.ToString() ?? "none"}";

	[Property]
	[Sync( SyncFlags.FromHost )]
	public Agent Agent
	{
		get => _owner;
		set
		{
			if ( _owner == value )
				return;

			if ( value != null && !AllowOwnership( value ) )
				return;

			var old = _owner;

			_owner = value;

			OnSetOwner( old, value );
		}
	}
	private Agent _owner;

	protected override void OnEnabled()
	{
		base.OnEnabled();

		UpdateNetworking();
	}

	public void UpdateNetworking()
	{
		GameObject.NetworkSetup(
			cn: Agent?.Connection,
			orphanMode: NetworkOrphaned.ClearOwner,
			ownerTransfer: OwnerTransfer.Fixed,
			netMode: NetworkMode.Object,
			ignoreProxy: true
		);
	}

	/// <summary>
	/// Called when the <see cref="Agent"/> property has been set to a new value.
	/// </summary>
	protected void OnSetOwner( Agent old, Agent agent )
	{
		if ( !Networking.IsHost )
			return;

		if ( old.IsValid() )
		{
			this.Log( agent.IsValid()
				? $"owner changed: {old} -> {agent}"
				: $"lost owner: {old}" );
		}
		else if ( agent.IsValid() )
		{
			this.Log( $"gained owner: {agent}" );
		}

		if ( old.IsValid() )
			old.RemovePawn( this );

		if ( agent.IsValid() )
		{
			var result = agent.AddPawn( this );

			if ( result is AttemptResult.Failure )
			{
				this.Warn( $"failed to add Pawn:{this} to Agent:{agent}" );
				Agent = null;
			}
			else if ( result is AttemptResult.Success )
			{
				OnAdded( old, agent );
			}
		}
	}

	/// <summary>
	/// Called when our new <see cref="Agent"/> has been fully confirmed.
	/// </summary>
	protected virtual void OnAdded( Agent old, Agent agent )
	{

	}

	/// <summary>
	/// Can an agent take ownership of this pawn?
	/// </summary>
	/// <returns> If ownership would be allowed. </returns>
	public virtual bool AllowOwnership( Agent agent )
	{
		if ( !agent.IsValid() )
			return false;

		if ( agent is not Client cl )
			return true;

		if ( !cl.IsValid() || !cl.Connected )
			return false;

		if ( Network.Owner is null || Network.Owner == cl.Connection )
			return true;

		return false;
	}

	public virtual bool CanSimulate()
		=> this.IsValid() && Network.IsOwner;

	public virtual void Simulate( in float deltaTime ) { }
}
