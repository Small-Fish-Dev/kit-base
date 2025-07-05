namespace GameFish;

/// <summary>
/// A trigger that starts with ladder tag and appropriate collider. <br />
/// Capable of creating, updating and previewing its collision.
/// </summary>
[Icon( "stairs" )]
[Title( "Ladder" )]
public partial class LadderTrigger : BaseTrigger
{
	[Property, Group( GROUP_COLLIDER )]
	public override ColliderType Collider
	{
		get => _colType;
		set { _colType = value; UpdateColliders(); }
	}
	private ColliderType _colType = ColliderType.Box;

	[ShowIf( nameof( UsingBox ), true )]
	[Property, Group( GROUP_COLLIDER )]
	public override BBox BoxSize
	{
		get => _boxSize;
		set { _boxSize = value; UpdateColliders(); }
	}
	private BBox _boxSize = new( new Vector3( 0, -16, 0f ), new Vector3( 12f, 16f, 256f ) );

	public override TagSet DefaultTags { get; } = [TAG_TRIGGER, TAG_LADDER];
	public override Color GizmoColor { get; } = Color.Orange.Desaturate( 0.3f ).Darken( 0.1f );
}
