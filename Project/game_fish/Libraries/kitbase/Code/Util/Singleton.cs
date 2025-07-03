namespace GameFish;

public static class SingletonUtility
{
	/// <summary>
	/// Retrieves the first valid instance of a component in the scene and caches it. <br />
	/// Call this on the private static instance in your public property's getter to auto-assign it.
	/// </summary>
	/// <returns> Null if in the editor or no instance was found. </returns>
	public static T GetSingleton<T>( this T instance ) where T : Component
	{
		if ( instance.IsValid() )
			return instance;

		if ( !Game.ActiveScene.IsValid() || Game.ActiveScene.IsEditor )
			return null;

		instance = Game.ActiveScene.GetAllComponents<T>().FirstOrDefault();

		return instance;
	}
}
