using PowBasics.CollectionsExt;
using ReactiveVars;
using RenderLib.Structs;
using RenderLib.Utils.DirectX.Exts;
using Brush = Vortice.Direct2D1.ID2D1Brush;

namespace RenderLib.Utils.DirectX;

sealed record PenNfo(
    Brush Brush,
    float Width,
    D2D.ID2D1StrokeStyle1 Style
) : IDisposable
{
    public void Dispose()
    {
        Brush.Dispose();
        Style.Dispose();
    }
}

sealed class Pencils : IDisposable
{
    private sealed record FontTextDef(FontDef Font, string Text);

    private readonly Disp d = new();
    public void Dispose() => d.Dispose();

    private readonly Dictionary<BrushDef, Brush> brushes;
    private readonly Dictionary<PenDef, PenNfo> pens;
    private readonly Dictionary<Bitmap, D2D.ID2D1Bitmap> bmps;

    private readonly Dictionary<FontDef, DWRITE.IDWriteTextFormat> fontsFormat;
    private readonly Dictionary<FontTextDef, DWRITE.IDWriteTextLayout> fontsLayout;


    private D2D.ID2D1Factory1 F { get; }
    private D2D.ID2D1RenderTarget T { get; }
    private DWRITE.IDWriteFactory7 W { get; }

    public Pencils(
        D2D.ID2D1Factory1 D2DFactory,
        D2D.ID2D1RenderTarget D2DDeviceCtx,
        DWRITE.IDWriteFactory7 DWRITEFactory
    )
    {
        F = D2DFactory;
        T = D2DDeviceCtx;
        W = DWRITEFactory;
        brushes = new Dictionary<BrushDef, Brush>().D(d);
        pens = new Dictionary<PenDef, PenNfo>().D(d);
        bmps = new Dictionary<Bitmap, D2D.ID2D1Bitmap>().D(d);
        fontsFormat = new Dictionary<FontDef, DWRITE.IDWriteTextFormat>().D(d);
        fontsLayout = new Dictionary<FontTextDef, DWRITE.IDWriteTextLayout>().D(d);
    }

    public Brush GetBrush(BrushDef def) => brushes.GetOrCreate(def, () => def switch
    {
        SolidBrushDef b => T.CreateSolidColorBrush(
            b.Color.ToColor(),
            new D2D.BrushProperties(1.0f)
        ),
        //BmpBrushDef b => new TextureBrush(b.Bmp),
        _ => throw new ArgumentException()
    });

    public PenNfo GetPen(PenDef def) => pens.GetOrCreate(def, () => new PenNfo(
        GetBrush(new SolidBrushDef(def.Color)),
        def.Width,
        F.CreateStrokeStyle(new D2D.StrokeStyleProperties1
        {
            DashStyle = (D2D.DashStyle)def.DashStyle,
            TransformType = D2D.StrokeTransformType.Hairline,
        })
    ));

    public D2D.ID2D1Bitmap GetBmp(Bitmap bmp) => bmps.GetOrCreate(bmp, () => BmpUtils.FromBmp(T, bmp));

    private DWRITE.IDWriteTextFormat GetFontFormat(FontDef def) => fontsFormat.GetOrCreate(def, () =>
    {
        var fontSizePixels = def.Size * 96 / 72;
        var textFormat = W.CreateTextFormat(
            def.Name,
            def.Bold ? DWRITE.FontWeight.Bold : DWRITE.FontWeight.Normal,
            def.Italic ? DWRITE.FontStyle.Italic : DWRITE.FontStyle.Normal,
            fontSizePixels
        );
        return textFormat;
    });


    public DWRITE.IDWriteTextLayout GetFontLayout(string text, FontDef def)
    {
        var fontTextDef = new FontTextDef(def, text);

        return fontsLayout.GetOrCreate(fontTextDef, () =>
        {
            var format = GetFontFormat(fontTextDef.Font);
            var textLayout = W.CreateTextLayout(fontTextDef.Text, format, float.MaxValue, float.MaxValue);
            return textLayout;
        });
    }
}
