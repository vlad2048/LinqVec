using System.Drawing.Drawing2D;
using PowBasics.CollectionsExt;
using ReactiveVars;
using RenderLib.Structs;

namespace RenderLib.Renderers.GDIPlus.Utils;

sealed class Pencils : IDisposable
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly Dictionary<BrushDef, Brush> brushes;
	private readonly Dictionary<PenDef, Pen> pens;
	private readonly Dictionary<FontDef, Font> fonts;

	public Pencils()
	{
		brushes = new Dictionary<BrushDef, Brush>().D(d);
		pens = new Dictionary<PenDef, Pen>().D(d);
		fonts = new Dictionary<FontDef, Font>().D(d);
	}

	public Brush GetBrush(BrushDef def) => brushes.GetOrCreate(def, () => def switch
	{
		SolidBrushDef b => new SolidBrush(b.Color),
		BmpBrushDef b => new TextureBrush(b.Bmp),
		_ => throw new ArgumentException()
	});

	public Pen GetPen(PenDef def) => pens.GetOrCreate(def, () =>
		new Pen(def.Color, def.Width)
		{
			DashStyle = (DashStyle)def.DashStyle
		}
	);

	public Font GetFont(FontDef def) => fonts.GetOrCreate(def, () => 
		new Font(
			def.Name,
			def.Size,
			(def.Bold, def.Italic) switch
			{
				(false, false) => FontStyle.Regular,
				(true, false) => FontStyle.Bold,
				(false, true) => FontStyle.Italic,
				(true, true) => FontStyle.Bold | FontStyle.Italic
			},
			GraphicsUnit.Point
		)
	);
}