namespace GameFish;

/// <summary>
/// Indicates something can stuff with variable timing if a check passes.
/// </summary>
public interface ISimulate
{
	public bool CanSimulate();
	public void Simulate( in float deltaTime );
}
