namespace CoolColorPicker.Utils;


sealed record Rgba(int R, int G, int B, int A);
sealed record Hsva(int Hue, int Sat, int Val, int Alpha);

static class ColorConvExt
{
	public static Hsva ToHsva(this Color e) => e.ToRgba().ToHsva();
	public static Rgba ToRgba(this Color e) => new(e.R, e.G, e.B, e.A);
	public static Color ToColor(this Hsva e) => e.ToRgba().ToColor();
	public static Color ToColor(this Rgba e) => Color.FromArgb(e.A, e.R, e.G, e.B);

	public static Hsva ToHsva(this Rgba e)
	{
		var (h, s, v) = rgb2hsv(
			ScaleFrom255(e.R),
			ScaleFrom255(e.G),
			ScaleFrom255(e.B)
		);
		return new Hsva(
			(int)h,
			ScaleTo100(s),
			ScaleTo100(v),
			e.A
		);
	}

	public static Rgba ToRgba(this Hsva e)
	{
		var (r, g, b) = hsv2rgb(
			e.Hue,
			ScaleFrom100(e.Sat),
			ScaleFrom100(e.Val)
		);
		return new Rgba(
			ScaleTo255(r),
			ScaleTo255(g),
			ScaleTo255(b),
			e.Alpha
		);
	}





	private static double Min(double a, double b, double c) => Math.Min(Math.Min(a, b), c);
	private static double Max(double a, double b, double c) => Math.Max(Math.Max(a, b), c);
	private static int ScaleTo255(double v) => Math.Min(255, Math.Max(0, (int)(v * 255)));
	private static int ScaleTo100(double v) => Math.Min(100, Math.Max(0, (int)(v * 100)));
	private static double ScaleFrom100(int v) => Math.Min(1.0, Math.Max(0.0, v / 100.0));
	private static double ScaleFrom255(int v) => Math.Min(1.0, Math.Max(0.0, v / 255.0));

	private static (double, double, double) rgb2hsv(double inR, double inG, double inB)
	{
		var min = Min(inR, inG, inB);
		var max = Max(inR, inG, inB);

		var outV = max;
		double outS;
		double outH;
		(double, double, double) Ret() => (outH, outS, outV);

		var delta = max - min;
		if (delta < 0.00001)
		{
			outS = 0;
			outH = 0;
			return Ret();
		}

		if (max > 0.0)
		{
			outS = delta / max;
		}
		else
		{
			outS = 0.0;
			outH = double.NaN;
			return Ret();
		}

		if (inR >= max)
			outH = (inG - inB) / delta;

		else if (inG >= max)
			outH = 2.0 + (inB - inR) / delta; // between cyan & yellow

		else
			outH = 4.0 + (inR - inG) / delta; // between magenta & cyan

		outH *= 60.0; // degrees

		if (outH < 0.0)
			outH += 360.0;

		return Ret();
	}




	private static (double, double, double) hsv2rgb(double inH, double inS, double inV)
	{
		double outR;
		double outG;
		double outB;

		(double, double, double) Ret() => (outR, outG, outB);

		if (inS <= 0.0)
		{
			// < is bogus, just shuts up warnings
			outR = inV;
			outG = inV;
			outB = inV;
			return Ret();
		}

		var hh = inH;
		if (hh >= 360.0) hh = 0.0;
		hh /= 60.0;
		var i = (long)hh;
		var ff = hh - i;
		var p = inV * (1.0 - inS);
		var q = inV * (1.0 - (inS * ff));
		var t = inV * (1.0 - (inS * (1.0 - ff)));

		switch (i)
		{
			case 0:
				outR = inV;
				outG = t;
				outB = p;
				break;
			case 1:
				outR = q;
				outG = inV;
				outB = p;
				break;
			case 2:
				outR = p;
				outG = inV;
				outB = t;
				break;

			case 3:
				outR = p;
				outG = q;
				outB = inV;
				break;
			case 4:
				outR = t;
				outG = p;
				outB = inV;
				break;
			default:
				outR = inV;
				outG = p;
				outB = q;
				break;
		}

		return Ret();
	}
}




/*
record Rgb(int R, int G, int B)
{
	public static readonly Rgb Default = new(255, 255, 255);
	public override string ToString() => $"rgb:{R:X2},{G:X2},{B:X2}";
}

record RgbPos(Rgb Rgb, Point Pos)
{
	public static readonly RgbPos Default = new(Rgb.Default, Point.Empty);
	public override string ToString() => $"{Rgb} pos:{Pos}";
}

record Hsv(int Hue, int Sat, int Val)
{
	public static readonly Hsv Default = new(0, 0, 100);
	public override string ToString() => $"hue:{Hue} sat:{Sat} Val:{Val}";
}

static class ColorUtils
{
	//public static Color WithAlpha(this Color c, int alpha) => Color.FromArgb(alpha, c.R, c.G, c.B);

	public static (Hsv, int) Col2HsvA(Color c)
	{
		var (h, s, v) = rgb2hsv(
			ScaleFrom255(c.R),
			ScaleFrom255(c.G),
			ScaleFrom255(c.B)
		);
		return (
			new Hsv(
				(int)h,
				ScaleTo100(s),
				ScaleTo100(v)
			),
			c.A
		);
	}

	public static Color HsvA2Col(Hsv hsv, int a)
	{
		var (r, g, b) = hsv2rgb(
			hsv.Hue,
			ScaleFrom100(hsv.Sat),
			ScaleFrom100(hsv.Val)
		);
		return Color.FromArgb(
			a,
			ScaleTo255(r),
			ScaleTo255(g),
			ScaleTo255(b)
		);
	}


	public static Rgb Hsv2Rgb(Hsv hsv)
	{
		var (r, g, b) = hsv2rgb(
			hsv.Hue,
			ScaleFrom100(hsv.Sat),
			ScaleFrom100(hsv.Val)
		);
		return new Rgb(
			ScaleTo255(r),
			ScaleTo255(g),
			ScaleTo255(b)
		);
	}




	private static double Min(double a, double b, double c) => Math.Min(Math.Min(a, b), c);
	private static double Max(double a, double b, double c) => Math.Max(Math.Max(a, b), c);
	private static int ScaleTo255(double v) => Math.Min(255, Math.Max(0, (int)(v * 255)));
	private static int ScaleTo100(double v) => Math.Min(100, Math.Max(0, (int)(v * 100)));
	private static double ScaleFrom100(int v) => Math.Min(1.0, Math.Max(0.0, v / 100.0));
	private static double ScaleFrom255(int v) => Math.Min(1.0, Math.Max(0.0, v / 255.0));

	private static (double, double, double) rgb2hsv(double inR, double inG, double inB)
	{
		var min = Min(inR, inG, inB);
		var max = Max(inR, inG, inB);

		var outV = max;
		double outS;
		double outH;
		(double, double, double) Ret() => (outH, outS, outV);

		var delta = max - min;
		if (delta < 0.00001)
		{
			outS = 0;
			outH = 0;
			return Ret();
		}

		if (max > 0.0)
		{
			outS = delta / max;
		}
		else
		{
			outS = 0.0;
			outH = double.NaN;
			return Ret();
		}

		if (inR >= max)
			outH = (inG - inB) / delta;

		else if (inG >= max)
			outH = 2.0 + (inB - inR) / delta; // between cyan & yellow

		else
			outH = 4.0 + (inR - inG) / delta; // between magenta & cyan

		outH *= 60.0; // degrees

		if (outH < 0.0)
			outH += 360.0;

		return Ret();
	}


	private static (double, double, double) hsv2rgb(double inH, double inS, double inV)
	{
		double outR;
		double outG;
		double outB;

		(double, double, double) Ret() => (outR, outG, outB);

		if (inS <= 0.0)
		{
			// < is bogus, just shuts up warnings
			outR = inV;
			outG = inV;
			outB = inV;
			return Ret();
		}

		var hh = inH;
		if (hh >= 360.0) hh = 0.0;
		hh /= 60.0;
		var i = (long)hh;
		var ff = hh - i;
		var p = inV * (1.0 - inS);
		var q = inV * (1.0 - (inS * ff));
		var t = inV * (1.0 - (inS * (1.0 - ff)));

		switch (i)
		{
			case 0:
				outR = inV;
				outG = t;
				outB = p;
				break;
			case 1:
				outR = q;
				outG = inV;
				outB = p;
				break;
			case 2:
				outR = p;
				outG = inV;
				outB = t;
				break;

			case 3:
				outR = p;
				outG = q;
				outB = inV;
				break;
			case 4:
				outR = t;
				outG = p;
				outB = inV;
				break;
			default:
				outR = inV;
				outG = p;
				outB = q;
				break;
		}

		return Ret();
	}
}
*/