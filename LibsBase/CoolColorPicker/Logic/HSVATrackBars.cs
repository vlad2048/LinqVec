using System.Reactive.Linq;
using CoolColorPicker.Structs;
using CoolColorPicker.Utils;

namespace CoolColorPicker.Logic;

sealed record Duo(TrackBar TrackBar, NumericUpDown UpDown)
{
	private bool ignoreUpdates;

	public IObservable<int> WhenChanged => Obs.Merge(
		TrackBar.Events().ValueChanged.Select(_ => TrackBar.Value).Where(_ => !ignoreUpdates).Do(e => UpDown.Value = e),
		UpDown.Events().ValueChanged.Select(_ => (int)UpDown.Value).Where(_ => !ignoreUpdates).Do(e => TrackBar.Value = e)
	);

	public void Set(int v)
	{
		ignoreUpdates = true;
		TrackBar.Value = v;
		UpDown.Value = v;
		ignoreUpdates = false;
	}
}

sealed class HSVATrackBars(
	Duo hue,
	Duo sat,
	Duo val,
	Duo alpha
)
{
	private Hsva curColor = Consts.DefaultColor.ToHsva();

	public IObservable<ColorUpdateEvt> WhenUpdate => Obs.Merge(
		hue.WhenChanged.Select(e => new ColorUpdateEvt(Src.HSVATrackBars, (curColor with { Hue = e }).ToRgba())),
		sat.WhenChanged.Select(e => new ColorUpdateEvt(Src.HSVATrackBars, (curColor with { Sat = e }).ToRgba())),
		val.WhenChanged.Select(e => new ColorUpdateEvt(Src.HSVATrackBars, (curColor with { Val = e }).ToRgba())),
		alpha.WhenChanged.Select(e => new ColorUpdateEvt(Src.HSVATrackBars, (curColor with { Alpha = e }).ToRgba()))
	);

	public void Set(ColorUpdateEvt evt)
	{
		curColor = evt.Color.ToHsva();
		hue.Set(curColor.Hue);
		sat.Set(curColor.Sat);
		val.Set(curColor.Val);
		alpha.Set(curColor.Alpha);
	}
}