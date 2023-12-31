using LinqVec.Tools;
using LinqVec.Utils;
using PowBasics.CollectionsExt;
using Point = System.Drawing.Point;

namespace LinqVec.Panes.ToolsPaneLogic_;


static class IconMapLoader
{
	public static Bitmap[] Load(ToolNfo tool) =>
		Enum.GetValues<ToolIconState>()
			.SelectToArray(state => MakeIcon(tool.Icon, tool.Name, state));


	private static Bitmap MakeIcon(Bitmap? baseIcon, string name, ToolIconState state) =>
		ToolIconUtils.Render(
			baseIcon ?? MakeBaseIconFromName(name),
			state
		);

	private static readonly Color textBackColor = Color.Transparent;
	private static readonly Font iconFont = new(FontFamily.GenericMonospace, 14, FontStyle.Bold, GraphicsUnit.Point);

	private static Bitmap MakeBaseIconFromName(string name)
	{
		var bmp = new Bitmap(16, 16);
		using var gfx = Graphics.FromImage(bmp);
		TextRenderer.DrawText(gfx, name, iconFont, new Point(-2, -3), Color.White, textBackColor, TextFormatFlags.PreserveGraphicsClipping);
		return bmp;
	}
}