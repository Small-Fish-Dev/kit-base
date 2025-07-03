namespace GameFish;

/// <summary>
/// A <see cref="PhysicsEntity"/> that does.. more stuff.
/// </summary>
public partial class Actor : PhysicsEntity, IAnimated, IRagdoll
{
    public const string FEATURE_ACTOR = "🎭 Actor";

    [Property, Feature( FEATURE_ACTOR )]
    public SkinnedModelRenderer AnimatedModel { get; set; }
    [Property, Feature( FEATURE_ACTOR )]
    public ModelPhysics Ragdoll { get; set; }
}
