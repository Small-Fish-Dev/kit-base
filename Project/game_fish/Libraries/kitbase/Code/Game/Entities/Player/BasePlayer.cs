namespace GameFish;

public /*abstract*/ partial class BasePlayer : BaseActor
{


	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		// TEMP: TESTING!
		// Settings.TrySet( "Benis", WorldTransform );
	}
}
