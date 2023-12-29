using Geom;
using LinqVec.Tools;
using LinqVec.Utils;

namespace LinqVec.Panes.ToolsPaneLogic_;

sealed record IconMap<TDoc>(
	IReadOnlyDictionary<(ITool<TDoc>, ToolIconState), Bitmap> State2Icon,
	IReadOnlyDictionary<ITool<TDoc>, R> Tool2IconR
);


static class IconMapLoader
{
	public static IconMap<TDoc> Load<TDoc>(ITool<TDoc>[] tools) =>
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

	private static readonly Brush normalBack = new SolidBrush(MkCol(0x353535));
	private static readonly Font iconFont = new(FontFamily.GenericMonospace, 18, FontStyle.Bold, GraphicsUnit.Point);

	private static Bitmap MakeBaseIconFromName(string name)
	{
		var bmp = new Bitmap(32, 32);
		var r = new Rectangle(0, 0, bmp.Width, bmp.Height);
		using var gfx = Graphics.FromImage(bmp);
		gfx.FillRectangle(normalBack, r);
		gfx.SetClip(new Rectangle(8, 8, 16, 16));
		TextRenderer.DrawText(gfx, name, iconFont, new Point(8, 8), Color.White, MkCol(0x353535), TextFormatFlags.PreserveGraphicsClipping);
		return bmp;
	}
}