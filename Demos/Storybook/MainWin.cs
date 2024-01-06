using System.Reactive.Linq;
using CoolColorPicker;
using LinqVec.Utils;
using LinqVec.Utils.Rx;
using ReactiveVars;
using Storybook.Logic;
using Storybook.Utils;
using UILib;

namespace Storybook;

sealed partial class MainWin : Form
{
	private PaletteKeeper PaletteKeeper => drawPanel.PaletteKeeper;

	public MainWin()
	{
		DoubleBuffered = true;
		InitializeComponent();

		this.InitRX(d =>
		{
			statusStrip.AddColor("fore", drawPanel.HoveredChunk.Map2(e => e.Fore).SelectMany(e => e), d);
			statusStrip.AddColor("back", drawPanel.HoveredChunk.Map2(e => e.Back).SelectMany(e => e), d);
			statusStrip.AddLabel("text", drawPanel.HoveredChunk.Map2IfNone(e => e.Text, "")).D(d);
			
			drawPanel.WhenColorClicked.Subscribe(e =>
			{
				using var dlg = new ColorPickerDialog();
				//dlg.Color.V = e.NamedColor.Color.FullAlpha();
				dlg.Color.V = PaletteKeeper.GetColorForDisplayNoOverride(e.NamedColor.Name);

				var dlgD = dlg.GetD();
				dlg.Color.Subscribe(f =>
				{
					PaletteKeeper.OverrideSet(e.NamedColor.Name, f.RemoveAlpha());
				}).D(dlgD);

				if (dlg.ShowDialog() == DialogResult.OK)
				{
					PaletteKeeper.OverrideAccept();
				}
				else
				{
					PaletteKeeper.OverrideReject();
				}

			}).D(d);

			drawPanel.Events().KeyDown.Subscribe(e =>
			{
				if (e.KeyCode != Keys.F2) return;
				PaletteKeeper.SaveChanges();
			}).D(d);
		});
	}
}