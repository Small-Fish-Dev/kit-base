namespace GameFish;

/// <summary>
/// Something with physics and an <see cref="ActorModel"/> that can take damage.
/// </summary>
public partial class BaseActor : PhysicsEntity
{
    public const string FEATURE_ACTOR = "🎭 Actor";

	/// <summary>
	/// The model of the actor, which may be <see cref="ActorSkinnedModel"/> or some other kind.
	/// </summary>
    [Property, Feature( FEATURE_ACTOR )]
    public ActorModel Model { get; set; }
}
