using LogLib.Structs;
using LogLib.Utils;
using LogLib.Writers;

namespace LogLib;


public static class B
{
	public static readonly Col gen_colWhite		= new(0xffffff, nameof(gen_colWhite));
	public static readonly Col gen_colBlack		= new(0x000000, nameof(gen_colBlack));
	public static readonly Col gen_colNeutral	= new(0x7c7c7c, nameof(gen_colNeutral));
	public static readonly Col gen_colOn		= new(0x5fba32, nameof(gen_colOn));
	public static readonly Col gen_colOff		= new(0x418520, nameof(gen_colOff));
}


public static class PrettyRenderExt
{
	public static void RenderFlag(this ITxtWriter w, bool val, string name) => w
		.Write(val ? "on " : "off", val ? B.gen_colOn : B.gen_colOff)
		.Surround($"[{name}:", "]", B.gen_colNeutral);


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

	private static readonly Col colTime0 = new(0x4ff03a, nameof(colTime0));
	private static readonly Col colTime1 = new(0xafe356, nameof(colTime1));
	private static readonly Col colTime2 = new(0xd2e055, nameof(colTime2));
	private static readonly Col colTime3 = new(0xdec754, nameof(colTime3));
	private static readonly Col colTime4 = new(0xdea74e, nameof(colTime4));
	private static readonly Col colTime5 = new(0xd17b41, nameof(colTime5));
	private static readonly Col colTime6 = new(0xd6583c, nameof(colTime6));
	private static readonly Col colTime7 = new(0xde4343, nameof(colTime7));
	private static Col ms2col(double ms)
	{
		if (ms < 005.0) return colTime0;
		if (ms < 010.0) return colTime1;
		if (ms < 020.0) return colTime2;
		if (ms < 040.0) return colTime3;
		if (ms < 080.0) return colTime4;
		if (ms < 150.0) return colTime5;
		if (ms < 300.0) return colTime6;
		return colTime7;
	}
}