namespace GameFish;

public abstract partial class BasePlayer : BasePawn
{
	public override bool IsDestructible { get => true; set { } }
}
