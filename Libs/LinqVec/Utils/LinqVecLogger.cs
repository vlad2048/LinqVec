using ReactiveVars;

namespace LinqVec.Utils;

public static class LinqVecLogger
{
	public static IDisposable AddLabel<T>(this StatusStrip strip, string label, IObservable<T> obs)
	{
		var d = MkD("AddLabel");
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