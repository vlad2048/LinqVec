using System.Drawing.Drawing2D;
using Geom;

namespace LinqVec.Structs;

/*

subscripts:
-----------
	s       screen space (pixels)
	g       grid (units)

***************************
* Ps = Pg * Zoom + Center *
***************************

Center:     location of the center of the grid in pixels from the top left corner of the client area

*/
public readonly record struct Transform(
	float ZoomBase,
    int ZoomIndex,
	Pt Center
)
{
	public float Zoom => ZoomBase * C.ZoomLevels[ZoomIndex];

    public Matrix Matrix => new(Zoom, 0, 0, Zoom, Center.X, Center.Y);

    public static readonly Transform Id = new(1, C.ZoomLevelOne, Pt.Zero);

	public static Transform MakeInitial(Pt clientSz)
    {
        var szPix = Math.Min(clientSz.X, clientSz.Y) - C.GridGfx.InitPaddingPx * 2;
        if (szPix <= 1) return Id;
        var szSys = C.Grid.TickSize * C.Grid.TickCount * 2;
        var result = new Transform(
            szPix / szSys,
            C.ZoomLevelOne,
			new Pt(
                clientSz.X / 2f,
                clientSz.Y / 2f
            )
        );
        if (result == Id) throw new ArgumentException("This shouldn't return Id as we use Id to represent no value");
        return result;
    }

    public override string ToString() => $"Zoom:{Zoom:F3}  Center:{Center}";
}

public static class TransformExt
{
	public static Pt ToGrid(this Pt p, Transform t) => (p - t.Center) * (1.0f / t.Zoom);
	public static Pt ToPixel(this Pt p, Transform t) => p * t.Zoom + t.Center;
	public static R ToGrid(this R r, Transform t) => new(r.Min.ToGrid(t), r.Max.ToGrid(t));
	public static R ToPixel(this R r, Transform t) => new(r.Min.ToPixel(t), r.Max.ToPixel(t));
}