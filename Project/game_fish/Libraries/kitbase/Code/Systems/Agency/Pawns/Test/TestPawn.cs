namespace GameFish;

partial class TestPawn : BasePawn
{
	public override void Simulate( in float deltaTime )
	{
		base.Simulate( deltaTime );

		if ( Input.Pressed( "Jump" ) )
			this.Log( "jumped" );
	}
}
