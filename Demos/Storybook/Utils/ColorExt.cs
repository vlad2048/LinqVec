namespace Storybook.Utils;

static class ColorExt
{
	public static Color FullAlpha(this Color c) => Color.FromArgb(255, c.R, c.G, c.B);
	public static Color RemoveAlpha(this Color c) => Color.FromArgb(0, c.R, c.G, c.B);
}