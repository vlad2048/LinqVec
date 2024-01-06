using LinqVec;
using LogLib.Structs;
using ReactiveVars;

namespace Storybook.Utils;

static class StatusExt
{
	public static IDisposable AddColor(this StatusStrip strip, string label, IObservable<Option<NamedColor>> obs, Disp d)
	{
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
			Width = 200,
		};
		var cc = labelValue.BackColor;
		obs
			//.ObserveOnUI()
			.Subscribe(v =>
			{
				labelValue.Text = v.Map(e => e.Name).IfNone("_");
				labelValue.BackColor = v.Map(e => e.Color).IfNone(Color.LightGray);
				labelValue.ForeColor = v.Map(e => e.Color).IfNone(Color.LightGray);
			}).D(d);
		strip.Items.AddRange(new ToolStripItem[]
		{
			labelHeader,
			labelValue
		});
		return d;
	}
}