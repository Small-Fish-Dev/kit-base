namespace GameFish;

partial class BasePawn : IOperate
{
	public virtual bool CanOperate()
		=> this.IsOwner();

	public virtual void FrameOperate( in float deltaTime ) { }
	public virtual void FixedOperate( in float deltaTime ) { }
}
