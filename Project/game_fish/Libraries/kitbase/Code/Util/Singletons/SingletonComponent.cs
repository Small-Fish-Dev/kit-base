namespace GameFish;

public abstract class Singleton<TComp> : Component where TComp : Component
{
	private static TComp _instance;

	/// <summary>
	/// The first active non-editor instance of <typeparamref name="TComp"/>.
	/// </summary>
	public static TComp Instance
	{
		get => _instance.GetSingleton();
		protected set => _instance = value;
	}

	/// <summary>
	/// The first active instance of <typeparamref name="TComp"/>. <br />
	/// This works even in in the editor.
	/// </summary>
	public static TComp EditorInstance
	{
		get => _instance.GetSingleton( allowEditor: true );
		protected set => _instance = value;
	}
}
