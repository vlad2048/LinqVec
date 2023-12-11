using System.Drawing.Drawing2D;
using Geom;
using LinqVec.Drawing;
using LinqVec.Structs;
using LinqVec.Utils;
using VectorEditor.Model;

namespace VectorEditor.Tools.Select_;

static class SelectPainter
{
	private static readonly GPen Pen = new(0x000000, 1, false, DashStyle.Dash);


	public static void DrawSelRect(
		Gfx gfx,
		IVisualObjSer obj
	)
	{
		var r = obj.BoundingBox.ToPixel(gfx.Transform).Enlarge(5);
		using (gfx.UsePixels())
			gfx.DrawR(r, Pen);
	}
}