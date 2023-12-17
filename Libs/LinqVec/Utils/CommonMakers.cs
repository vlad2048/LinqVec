namespace LinqVec.Utils;

public static class CommonMakers
{
	public static Color MkCol(int v) => Color.FromArgb(0xFF, Color.FromArgb(v));
	public static Color MkCol(uint v) => Color.FromArgb(0xFF, Color.FromArgb((int)v));
}
