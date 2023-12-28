<Query Kind="Program">
  <Namespace>System.Drawing</Namespace>
</Query>

void Main()
{
	var folderIn = @"C:\dev\big\LinqVec\_infos\art\tool-icons\baked";
	var folderOut = @"C:\dev\big\LinqVec\_infos\art\tool-icons";
	var cBack = MkCol(0x353535);
	Color[] cFores = [MkCol(0xEBEBEB), MkCol(0x00990A)];
	
	BmpUtils.ReconstructAlpha(folderIn, folderOut, cBack, cFores);
}


public static class BmpUtils
{
	public static void ReconstructAlpha(string folderIn, string folderOut, Color cBack, Color[] cFores)
	{
		var filesIn = Directory.GetFiles(folderIn, "*.png");
		foreach (var fileIn in filesIn)
		{
			var fileOut = Path.Combine(folderOut, Path.GetFileName(fileIn));
			var bmp = ReconstructAlpha(fileIn, cBack, cFores);
			bmp.Save(fileOut);
			bmp.Dump();
		}
	}



	private static Bitmap ReconstructAlpha(string file, Color cBack, Color[] cFores)
	{
		var bmpIn = new Bitmap(file);
		var bmpOut = new Bitmap(bmpIn.Width, bmpIn.Height);
		for (var x = 0; x < bmpIn.Width; x++)
		for (var y = 0; y < bmpIn.Height; y++)
		{
			var cIn = bmpIn.GetPixel(x, y);
			Color cOut;
			if (cIn == cBack)
			{
				cOut = Color.Transparent;
			}
			else
			{
				var alphaRes = ReconstructAlpha(cIn, cBack, cFores);
				cOut = alphaRes.COut;
			}
			bmpOut.SetPixel(x, y, cOut);
		}
		return bmpOut;
	}

	private sealed record AlphaRes(
		(byte, byte, byte) Alpha,
		Color CBack,
		Color CFore
	)
	{
		public byte Metric => (byte)(Max(Alpha.Item1, Alpha.Item2, Alpha.Item3) - Min(Alpha.Item1, Alpha.Item2, Alpha.Item3));
		
		public Color COut
		{
			get
			{
				//var alpha = (byte)((Alpha.Item1 + Alpha.Item2 + Alpha.Item3) / 3);
				var alpha = Min(Alpha.Item1, Alpha.Item2, Alpha.Item3);
				return Color.FromArgb(
					alpha,
					//(byte)(CBack.R + alpha * (CFore.R - CBack.R) / 255),
					//(byte)(CBack.G + alpha * (CFore.G - CBack.G) / 255),
					//(byte)(CBack.B + alpha * (CFore.B - CBack.B) / 255)
					CFore.R,
					CFore.G,
					CFore.B
				);
			}
		}
	}

	private static AlphaRes ReconstructAlpha(Color cIn, Color cBack, Color[] cFores) =>
		cFores
			.Select(cFore => ReconstructAlpha(cIn, cBack, cFore))
			.MinBy(e => e.Metric)!;
	
	private static AlphaRes ReconstructAlpha(Color cIn, Color cBack, Color cFore) => new(
		(
			ReconstructAlpha(cIn.R, cBack.R, cFore.R),
			ReconstructAlpha(cIn.G, cBack.G, cFore.G),
			ReconstructAlpha(cIn.B, cBack.B, cFore.B)
		),
		cBack,
		cFore
	);
	
	private static byte ReconstructAlpha(byte cIn, byte cBack, byte cFore)
	{
		var alpha = (byte)( (cIn - cBack) * 255.0 / (cFore - cBack) );
		return alpha;
	}
	
	private static byte Min(byte x, byte y, byte z) => Math.Min(Math.Min(x, y), z);
	private static byte Max(byte x, byte y, byte z) => Math.Max(Math.Max(x, y), z);
}



public static Color MkCol(uint v) => Color.FromArgb(0xFF, Color.FromArgb((int)v));


static object ToDump(object o) => o switch
{
	Color e => $"0x{e.R:X2}{e.G:X2}{e.B:X2}({e.A:X2})",
	_ => o
};