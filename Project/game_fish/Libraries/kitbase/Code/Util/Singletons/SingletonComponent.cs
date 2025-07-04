namespace GameFish;

public abstract class Singleton<TComp> : Component where TComp : Component
{
	/// <summary>
	/// The first active non-editor instance of <typeparamref name="TComp"/>.
	/// </summary>
	public static TComp Instance
	{
		get => _instance.GetSingleton();
		protected set => _instance = value;
	}
	static TComp _instance;

	/// <summary>
	/// The first instance of <typeparamref name="TComp"/>. <br />
	/// This works even in in the editor.
	/// </summary>
	public static TComp EditorInstance
	{
		get => _editorInstance.GetSingleton( allowEditor: true );
		protected set => _editorInstance = value;
	}
	static TComp _editorInstance;
}
