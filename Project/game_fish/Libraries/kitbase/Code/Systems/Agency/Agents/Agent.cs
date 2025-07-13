namespace GameFish;

/// <summary>
/// Something capable of control over other objects. <br />
/// It may be a player(real/fake) or an NPC.
/// </summary>
[Icon( "psychology" )]
[EditorHandle( Icon = "psychology" )]
public abstract partial class Agent : Component, ISimulate
{
	public const string FEATURE_AGENT = "🧠 Agent";
	public const string GROUP_ID = "🆔 Identity";

	/// <summary>
	/// Is this meant to be owned by a player?
	/// </summary>
	[Property, Feature( FEATURE_AGENT )]
	public virtual bool IsPlayer { get; } = false;

	/// <summary>
	/// Which pawns are known to be under this agent's control?
	/// </summary>
	[Sync( SyncFlags.FromHost )]
	[Property, ReadOnly, Feature( FEATURE_AGENT )]
	public NetList<Pawn> Pawns { get; set; }

	public virtual Identity Identity { get; protected set; }
	public virtual Connection Connection => null;

	/// <summary>
	/// If NPC/Bot: always true. ('cause they in the matrix or some shit) <br />
	/// If Client: if the connection exists and is active.
	/// </summary>
	public virtual bool Connected => true;

	/// <summary>
	/// If NPC/Bot: always false. <br />
	/// If Client: if our <see cref="Identity"/> has the specified connection.
	/// </summary>
	public virtual bool CompareConnection( Connection cn )
		=> false;

	// public override string ToString()
	// => $"{GetType().ToSimpleString( includeNamespace: false )}";

	/// <summary>
	/// The display name of this guy/gal/whatever.
	/// </summary>
	public virtual string Name
		=> GetType().ToSimpleString();

	protected override void OnUpdate()
	{
		base.OnUpdate();

		Simulate( Time.Delta );
	}

	public virtual Pawn.AttemptResult AddPawn( Pawn pawn )
	{
		if ( !Networking.IsHost )
			return Pawn.AttemptResult.Failure;

		if ( !this.IsValid() || !Scene.IsValid() || Scene.IsEditor )
			return Pawn.AttemptResult.Failure;

		if ( !pawn.IsValid() )
			return Pawn.AttemptResult.Failure;

		if ( IsPlayer && Identity.Type is ClientType.User )
		{
			// Must have an active, valid connection.
			if ( !Connected )
				return Pawn.AttemptResult.Failure;

			var cn = Connection;

			if ( cn is null )
				this.Warn( "had a null connection while taking a pawn" );

			if ( !pawn.Network.IsOwner )
				if ( !pawn.Network.AssignOwnership( Connection ) )
					return Pawn.AttemptResult.Failure;
		}

		if ( Pawns is null )
		{
			Pawns = [pawn];
		}
		else if ( !Pawns.Contains( pawn ) )
		{
			Pawns.Add( pawn );
		}

		ValidatePawns();

		OnGainPawn( pawn );

		return Pawn.AttemptResult.Success;
	}

	public virtual void RemovePawn( Pawn pawn )
	{
		if ( !Networking.IsHost )
		{
			this.Warn( $"tried to remove Pawn:[{pawn}] from Agent:[{this}] as non-host" );
			return;
		}

		if ( !this.IsValid() || !Scene.IsValid() || Scene.IsEditor )
			return;

		if ( pawn is null )
			return;

		if ( pawn.IsValid() && pawn.Network.IsOwner )
			pawn.Network.DropOwnership();

		ValidatePawns();

		OnLosePawn( pawn );
	}

	/// <summary>
	/// Called after a pawn we owned was confirmed to be removed.
	/// </summary>
	protected virtual void OnLosePawn( Pawn pawn )
	{
	}

	/// <summary>
	/// Called after a pawn we didn't own is confirmed to be owned.
	/// </summary>
	protected virtual void OnGainPawn( Pawn pawn )
	{
	}

	/// <summary>
	/// Clears out references to invalid or unowned pawns.
	/// </summary>
	protected virtual void ValidatePawns()
	{
		if ( !Networking.IsHost )
			return;

		if ( !this.IsValid() || !Scene.IsValid() || Scene.IsEditor )
			return;

		if ( Pawns is null )
			return;

		var toRemove = new List<Pawn>();

		foreach ( var pawn in Pawns )
			if ( !pawn.IsValid() || pawn.Agent != this )
				toRemove.Add( pawn );

		toRemove.ForEach( cl => Pawns.Remove( cl ) );
	}

	/// <summary>
	/// Sends a request to the host to take a pawn.
	/// </summary>
	public Pawn.AttemptResult RequestTakePawn( Pawn pawn )
	{
		if ( !pawn.IsValid() || !pawn.AllowOwnership( this ) )
			return Pawn.AttemptResult.Failure;

		RpcRequestTakePawn( pawn );

		return Pawn.AttemptResult.Request;
	}

	[Rpc.Host( NetFlags.Reliable | NetFlags.OwnerOnly )]
	protected void RpcRequestTakePawn( Pawn pawn )
	{
		TryTakePawn( pawn );
	}

	public virtual Pawn.AttemptResult TryTakePawn( Pawn pawn )
	{
		// Only hosts can process pawn takeover attempts.
		if ( !Network.IsOwner )
		{
			RpcRequestTakePawn( pawn );
			return Pawn.AttemptResult.Request;
		}

		if ( !pawn.IsValid() || !pawn.AllowOwnership( this ) )
			return Pawn.AttemptResult.Failure;

		if ( !Connected )
			return Pawn.AttemptResult.Failure;

		pawn.Agent = this;

		RpcTryTakePawnHostResponse( pawn, Pawn.AttemptResult.Success );

		return Pawn.AttemptResult.Success;
	}

	/// <summary>
	/// This is the method you want to call so the host can tell the owner what happened.
	/// </summary>
	[Rpc.Owner( NetFlags.Reliable | NetFlags.HostOnly )]
	protected void RpcTryTakePawnHostResponse( Pawn pawn, Pawn.AttemptResult result )
	{
		if ( pawn.IsValid() )
			OnTryTakePawnResponse( pawn, result );
	}

	protected virtual void OnTryTakePawnResponse( Pawn pawn, in Pawn.AttemptResult result )
	{
	}

	/// <returns> If this computer can tell these pawns what to do. </returns>
	public virtual bool CanSimulate()
	{
		if ( !this.IsValid() || !Scene.IsValid() || Scene.IsEditor )
			return false;

		return !IsProxy;
	}

	public virtual void Simulate( in float deltaTime )
	{
		if ( !CanSimulate() )
			return;

		SimulatePawns( deltaTime );
	}

	protected virtual void SimulatePawns( in float deltaTime )
	{
		if ( Pawns is null )
			return;

		foreach ( var pawn in Pawns )
			if ( pawn.CanSimulate() )
				pawn.Simulate( in deltaTime );
	}
}
