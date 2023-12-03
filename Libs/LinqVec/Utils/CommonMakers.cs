namespace LinqVec.Utils;

public static class CommonMakers
{
	public static Color MkCol(uint v) => Color.FromArgb(0xFF, Color.FromArgb((int)v));
	internal static PtInt MkPt(int x, int y) => new(x, y);
    internal static RInt MkR(int x, int y, int width, int height) => new(MkPt(x, y), MkPt(x + width, y + height));
}
