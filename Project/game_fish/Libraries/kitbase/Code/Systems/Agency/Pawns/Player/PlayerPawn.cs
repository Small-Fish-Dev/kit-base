namespace GameFish;

/// <summary>
/// A simple player-only pawn that uses the <see cref="PlayerController"/>.
/// </summary>
public partial class PlayerPawn : BasePawn
{
	public const string GROUP_PLAYER = "Player";

	[Property]
	[Feature( FEATURE_PAWN ), Group( GROUP_PLAYER )]
	public PlayerController Controller
	{
		get => _pc.IsValid() ? _pc : Components?.Get<PlayerController>( FindMode.EverythingInSelf );
		set { _pc = value; }
	}
	private PlayerController _pc;

	public virtual Vector3 WishVelocity
	{
		get => _pc.IsValid() ? _pc.WishVelocity : default;
		set { if ( _pc.IsValid() ) _pc.WishVelocity = value; }
	}

	protected override void OnStart()
	{
		base.OnStart();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		var allowInput = CanOperate();

		if ( Controller.IsValid() )
		{
			Controller.UseInputControls = allowInput;
		}
	}

	protected override void OnSetOwner( Agent old, Agent agent )
	{
		base.OnSetOwner( old, agent );

		if ( !agent.IsValid() )
			WishVelocity = default;
	}

	/// <summary>
	/// Only player <see cref="Agent"/>s can own a player pawn.
	/// </summary>
	public override bool AllowOwnership( Agent agent )
	{
		if ( !base.AllowOwnership( agent ) )
			return false;

		return agent?.IsPlayer ?? false;
	}

	public override void FrameOperate( in float deltaTime )
	{
		base.FrameOperate( deltaTime );
	}

	public override void FixedOperate( in float deltaTime )
	{
		base.FixedOperate( deltaTime );
	}
}
