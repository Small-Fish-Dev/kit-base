namespace GameFish;

partial class Trigger : Component.IHasBounds
{
	// TEMP: Just so we can resize shit at least somewhat properly.
	public BBox LocalBounds => BBox.FromPositionAndSize( default, 512f );
}
