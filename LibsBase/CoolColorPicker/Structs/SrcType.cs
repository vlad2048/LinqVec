using CoolColorPicker.Utils;

namespace CoolColorPicker.Structs;

enum Src
{
	ColorOutput,
	HSVATrackBars,
	RGBPanel,
}


sealed record ColorUpdateEvt(
	Src Src,
	Rgba Color
);
