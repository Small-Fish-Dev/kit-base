namespace GameFish;

partial class TestPawn : Pawn
{
	public override void Simulate( in float deltaTime )
	{
		base.Simulate( deltaTime );

		if ( Input.Pressed( "Jump" ) )
			this.Log( "jumped" );
	}
}
