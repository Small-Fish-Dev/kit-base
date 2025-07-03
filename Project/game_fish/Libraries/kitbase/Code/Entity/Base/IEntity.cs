namespace GameFish;

/// <summary>
/// The most basic form of a physical object that can separately exist. <br />
/// This is an interface so that non-components can be managed as an entity.
/// </summary>
public interface IEntity : ITransform, IHealth
{
}
