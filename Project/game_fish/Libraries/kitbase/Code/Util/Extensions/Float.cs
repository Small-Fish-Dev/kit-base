using System;

namespace GameFish;

public static partial class GameFish
{
	public static float Abs( this float n )
		=> MathF.Abs( n );

	public static int Sign( this float n )
		=> MathF.Sign( n );
}
