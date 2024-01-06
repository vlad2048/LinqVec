using LogLib.Structs;
using LogLib.Utils;
using LogLib.Writers;

namespace LogLib;


public static class PrettyRenderExt
{
	public static void RenderFlag(this ITxtWriter w, bool val, string name) => w
		.Write(val ? "on " : "off", val ? S.General.On : S.General.Off)
		.Surround($"[{name}:", "]", S.General.Neutral);


	public static IChunk[] RenderDeltaTime(this TimeSpan t)
	{
		var w = new MemoryTxtWriter();
		w.RenderDeltaTime(t);
		return w.Chunks;
	}

	private static void RenderDeltaTime(this ITxtWriter w, TimeSpan t)
	{
		var ms = t.TotalMilliseconds;
		w
			.Write("[" + $"{(int)ms}ms".PadLeft(7) + "]", ms2col(ms))
			.spc(1);
	}

	private static NamedColor ms2col(double ms)
	{
		if (ms < 005.0) return S.LogTicker.Time.Val0;
		if (ms < 010.0) return S.LogTicker.Time.Val1;
		if (ms < 020.0) return S.LogTicker.Time.Val2;
		if (ms < 040.0) return S.LogTicker.Time.Val3;
		if (ms < 080.0) return S.LogTicker.Time.Val4;
		if (ms < 150.0) return S.LogTicker.Time.Val5;
		if (ms < 300.0) return S.LogTicker.Time.Val6;
		return S.LogTicker.Time.Val7;
	}
}