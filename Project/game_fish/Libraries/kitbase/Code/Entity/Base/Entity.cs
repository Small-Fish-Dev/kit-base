namespace GameFish;

/// <summary>
/// The most basic form of a physical object that can separately exist.
/// </summary>
public partial class Entity : Component, ITransform, IHealth
{
	public const string FEATURE_ENTITY = "📦 Entity";
	public const string FEATURE_NPC = "🤖 NPC";

	public const string TAG_ENTITY = "entity";
	public const string TAG_PROJECTILE = "projectile";

	public const string TAG_ACTOR = "actor";
	public const string TAG_PLAYER = "player";
	public const string TAG_NPC = "npc";
}
