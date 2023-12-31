using Geom;
using LinqVec.Tools;
using LinqVec.Utils;
using Point = System.Drawing.Point;

namespace LinqVec.Panes.ToolsPaneLogic_;

sealed record IconMap<TDoc, TState>(
	IReadOnlyDictionary<(ITool<TDoc, TState>, ToolIconState), Bitmap> State2Icon,
	IReadOnlyDictionary<ITool<TDoc, TState>, R> Tool2IconR
);


static class IconMapLoader
{
	public static IconMap<TDoc, TState> Load<TDoc, TState>(ITool<TDoc, TState>[] tools) =>
		new(
			(
				from tool in tools
				from state in Enum.GetValues<ToolIconState>()
				let img = MakeIcon(tool.Icon, tool.Name, state)
				select ((tool, state), img)
			)
			.ToDictionary(e => e.Item1, e => e.img),
			tools
				.Select((tool, idx) =>
					(
						tool,
						R.Make(
							idx % 2 * 32,
							// ReSharper disable once PossibleLossOfFraction
							idx / 2 * 32,
							32,
							32
						)
					)
				)
				.ToDictionary(e => e.tool, e => e.Item2)
		);



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