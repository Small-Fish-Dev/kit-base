namespace GameFish;

public partial class PlayerPawn : BasePawn
{
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
	}

	public override void FixedOperate( in float deltaTime )
	{
		base.FixedOperate( deltaTime );
	}
}
