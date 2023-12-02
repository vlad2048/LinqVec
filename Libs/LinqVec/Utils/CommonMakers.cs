namespace LinqVec.Utils;

static class CommonMakers
{
	public static Color MkCol(uint v) => Color.FromArgb(0xFF, Color.FromArgb((int)v));
	public static PtInt MkPt(int x, int y) => new(x, y);
    public static RInt MkR(int x, int y, int width, int height) => new(MkPt(x, y), MkPt(x + width, y + height));
}
