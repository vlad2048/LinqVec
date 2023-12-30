using ReactiveVars;

namespace LinqVec.Utils;

public static class ColoredLogger
{
	private const uint DefaultColor = 0xCCCCCC;


	public static Action<Func<Gizmo, Gizmo>> Log<Gizmo>(this Action<Func<Gizmo, Gizmo>> applyFunPrev, string name)
	{
		Action<Func<Gizmo, Gizmo>> applyFunNext = fPrev =>
		{
			Func<Gizmo, Gizmo> fNext = vPrev =>
			{
				var vNext = fPrev(vPrev);
				L.WriteLine($"[gizmo - {name}]: {vPrev} -> {vNext}");
				return vNext;
			};
			applyFunPrev(fNext);
		};
		return applyFunNext;
	}


	public static void Write(string s, int col)
	{
		WinAPI.Utils.ConUtils.SetColor(MkCol(col));
		Console.Write(s);
		WinAPI.Utils.ConUtils.SetColor(MkCol(DefaultColor));
	}

	public static void WriteLine(string s, int col)
	{
		WinAPI.Utils.ConUtils.SetColor(MkCol(col));
		Console.WriteLine(s);
		WinAPI.Utils.ConUtils.SetColor(MkCol(DefaultColor));
	}

	public static IDisposable AddLabel<T>(this StatusStrip strip, string label, IObservable<T> obs)
	{
		var d = MkD();
		var labelHeader = new ToolStripLabel($"{label}:")
		{
			Font = C.Fonts.MonoHeader,
		};
		var labelValue = new ToolStripStatusLabel("")
		{
			Font = C.Fonts.MonoValue,
			BorderSides = ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom,
			BorderStyle = Border3DStyle.Raised,
			Margin = new Padding(-5, 0, 5, 0),
		};
		obs
			//.ObserveOnUI()
			.Subscribe(v => labelValue.Text = $"{v}").D(d);
		strip.Items.AddRange(new ToolStripItem[]
		{
			labelHeader,
			labelValue
		});
		return d;
	}
}