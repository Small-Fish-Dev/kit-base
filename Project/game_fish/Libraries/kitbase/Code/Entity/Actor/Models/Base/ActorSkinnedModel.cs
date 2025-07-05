namespace GameFish;

public abstract partial class ActorSkinnedModel : ActorModel, ISkinned, IRagdoll
{
	[Property, Feature( Actor.FEATURE_ACTOR ), Group( GROUP_MODEL )]
	public SkinnedModelRenderer SkinnedModel { get; set; }

	[Property, Feature( Actor.FEATURE_ACTOR ), Group( GROUP_MODEL )]
	public ModelPhysics Ragdoll { get; set; }
}
