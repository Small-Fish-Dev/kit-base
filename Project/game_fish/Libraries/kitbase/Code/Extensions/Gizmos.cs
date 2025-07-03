namespace GameFish;

public static partial class GizmoExtensions
{
	/// <summary>
	/// Draws a slightly fancy box that shows depth.
	/// </summary>
	/// <param name="comp"> The component drawing this. </param>
	/// <param name="box"> The local bounds of the box. </param>
	/// <param name="c"> The default color. </param>
	/// <param name="tWorld"> The world transform to use. Defaults to the component's world transform. </param>
	/// <returns> If the box could be drawn. </returns>
	public static bool DrawBox( this Component comp, in BBox box, Color c, Transform? tWorld = null )
	{
		if ( !comp.IsValid() )
			return false;

		using ( Gizmo.Scope( comp.Id + "-box", Transform.Zero ) )
		{
			Gizmo.Transform = tWorld ??= comp.WorldTransform;

			var isSelected = Gizmo.IsSelected;

			// Draw a box underneath and through walls to better show depth.
			Gizmo.Draw.IgnoreDepth = true;
			Gizmo.Draw.LineThickness = 1f;
			Gizmo.Draw.Color = c.WithAlphaMultiplied( isSelected ? 0.2f : 0.1f );

			Gizmo.Draw.LineBBox( box );

			// Main box.
			Gizmo.Draw.IgnoreDepth = false;
			Gizmo.Draw.LineThickness = isSelected ? 2f : 1f;
			Gizmo.Draw.Color = c;

			Gizmo.Draw.LineBBox( box );
		}

		return true;
	}
}
