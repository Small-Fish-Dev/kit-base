namespace GameFish;

public static partial class GameFish
{
	public static Vector3 Direction( this Vector3 from, in Vector3 to ) => Vector3.Direction( from, to );

	/// <summary>
	/// B - A = Δ <br />
	/// Add this to <paramref name="from"/> to get <paramref name="to"/>.
	/// </summary>
	public static Vector3 Offset( this Vector3 from, in Vector3 to ) => to - from;
}
