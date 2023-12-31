using LinqVec.Tools;
using System.Reactive.Linq;
using Geom;
using LinqVec.Panes.ToolsPaneLogic_;
using LinqVec.Utils;
using ReactiveVars;
using UILib;
using WeifenLuo.WinFormsUI.Docking;
using PowBasics.CollectionsExt;

namespace LinqVec.Panes;


public sealed partial class ToolsPane : DockContent
{
	private sealed record ToolGfx(
		ITool Tool,
		R R
	);
	

	public ToolsPane(IRoVar<ITool[]> toolSet, IRoVar<ITool> curTool, Action<ITool> setCurTool)
	{
		DoubleBuffered = true;
		InitializeComponent();

		var tooltip = new ToolTip();

		this.InitRX(d =>
		{
			var mousePos =
				this.Events().MouseMove
					.Select(e => new Pt(e.X, e.Y))
					.Prepend(new Pt(-1, -1))
					.ToVar(d);
			var isDown =
				Obs.Merge(
						this.Events().MouseDown.Select(_ => true),
						this.Events().MouseUp.Select(_ => false)
					)
					.Prepend(false)
					.ToVar(d);

			var tools = toolSet.Select(e => e.Select((f, idx) => new ToolGfx(f, PaintUtils.GetR(idx))).ToArr()).ToVar(d);

			var hoveredTool =
				Obs.CombineLatest(
						tools,
						mousePos,
						(tools_, mousePos_) => (tools_, mousePos_)
					)
					.Select(t => t.tools_.FirstOrOption(e => e.R.Contains(t.mousePos_)))
					.Prepend(None)
					.DistinctUntilChanged()
					.ToVar(d);

			hoveredTool.Subscribe(e => e.Match(
				t =>
				{
					tooltip.SetToolTip(this, t.Tool.Nfo.Name);
					tooltip.Active = true;
				},
				() => tooltip.Active = false
			)).D(d);


			var stateMap =
				Obs.CombineLatest(
						tools,
						curTool,
						hoveredTool,
						mousePos,
						isDown,
						(tools_, curTool_, hoveredTool_, mousePos_, isDown_) => (tools_, curTool_, hoveredTool_, mousePos_, isDown_)
					)
					.Select(t =>
						t.tools_
							.Select(
								tool => (
									tool,
									(tool.Tool == curTool.V) switch
									{
										true => ToolIconState.Active,
										false => (Some(tool) == t.hoveredTool_, t.isDown_) switch
										{
											(true, true) => ToolIconState.MouseDown,
											(true, false) => ToolIconState.Hover,
											_ => ToolIconState.Normal,
										}
									}
								)
							)
							.ToHashMap()
					)
					.Prepend(new HashMap<ToolGfx, ToolIconState>())
					.DistinctUntilChanged()
					.ToVar(d);

			stateMap.Subscribe(_ => Invalidate()).D(d);

			this.Events().Click
				.Subscribe(_ =>
					tools.V.FirstOrOption(t => t.R.Contains(mousePos.V))
						.Select(t => t.Tool)
						.IfSome(setCurTool)
				).D(d);


			this.Events().Paint.Subscribe(e =>
			{
				var gfx = e.Graphics;
				var r = new Rectangle(0, 0, ClientSize.Width, ClientSize.Height);
				gfx.FillRectangle(PaintUtils.BackBrush, r);

				foreach (var tool in tools.V)
				{
					var state = stateMap.V.ContainsKey(tool) switch {
						true => stateMap.V[tool],
						false => ToolIconState.Normal
					};
					var bmp = PaintUtils.GetBmp(tool.Tool.Nfo, state);
					var toolR = tool.R;
					gfx.DrawImage(bmp, toolR.Min.X, toolR.Min.Y);
				}
			}).D(d);
		});
	}
}



file static class PaintUtils
{
	private static readonly Dictionary<ToolNfo, Bitmap[]> bmpCache = new();

	public static readonly Brush BackBrush = new SolidBrush(MkCol(0x353535));

	public static R GetR(int idx) => R.Make(
		idx % 2 * 32,
		// ReSharper disable once PossibleLossOfFraction
		idx / 2 * 32,
		32,
		32
	);

	public static Bitmap GetBmp(ToolNfo tool, ToolIconState state) =>
		bmpCache.GetOrCreate(tool, () => IconMapLoader.Load(tool))[(int)state];
}