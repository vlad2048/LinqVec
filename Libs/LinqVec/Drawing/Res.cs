using LinqVec.Structs;
using PowBasics.CollectionsExt;
using PowRxVar;

namespace LinqVec.Drawing;

public class Res : IDisposable
{
    private readonly Disp d = new();
    public void Dispose() => d.Dispose();

    private record struct PenNfo(Color Color, float Thickness, float Scale);
    private readonly Dictionary<Color, Brush> brushMap;
    private readonly Dictionary<PenNfo, Pen> penMap;

    public Res()
    {
        brushMap = new Dictionary<Color, Brush>().D(d);
        penMap = new Dictionary<PenNfo, Pen>().D(d);
    }

    public Brush Brush(Color color) => brushMap.GetOrCreate(color, () => new SolidBrush(color));
    public Pen Pen(Color color, float thickness) => penMap.GetOrCreate(new PenNfo(color, thickness, 1), () => new Pen(color, thickness));
    public Pen Pen(Color color, float thickness, float transformZoom) => penMap.GetOrCreate(new PenNfo(color, thickness, 1 / transformZoom), () =>
    {
        var pen = new Pen(color, thickness);
        pen.Transform = new Transform(1 / transformZoom, C.ZoomLevelOne, Pt.Zero).Matrix;
        return pen;
    });
}