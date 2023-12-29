using LinqVec.Tools;
using System.Reactive.Linq;
using Geom;
using LinqVec.Panes.ToolsPaneLogic_;
using LinqVec.Utils;
using ReactiveVars;
using UILib;
using WeifenLuo.WinFormsUI.Docking;

namespace LinqVec.Panes
{
	public sealed partial class ToolsPane<TDoc> : DockContent
	{
		private static Lazy<IconMap<TDoc>>? iconMap;
		private IconMap<TDoc> IconMap => (iconMap ?? throw new NullReferenceException()).Value;

		public ToolsPane(ITool<TDoc>[] tools, IRoVar<ITool<TDoc>> curTool, Action<ITool<TDoc>> setCurTool)
		{
			iconMap ??= new(() => IconMapLoader.Load(tools));
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

				var hoveredTool = mousePos
					.Select(e => tools.FirstOrOption(f => IconMap.Tool2IconR[f].Contains(e)))
					.Prepend(None)
					.DistinctUntilChanged()
					.ToVar(d);

				hoveredTool.Subscribe(e => e.Match(
					t =>
					{
						tooltip.SetToolTip(this, t.GetType().Name);
						tooltip.Active = true;
					},
					() => tooltip.Active = false
				)).D(d);


				var stateMap =
					Obs.CombineLatest(
							curTool,
							hoveredTool,
							mousePos,
							isDown,
							(curTool_, hoveredTool_, mousePos_, isDown_) => (curTool_, hoveredTool_, mousePos_, isDown_)
						)
						.Select(t =>
							tools
								.Select(
									tool => (
										tool,
										(tool == curTool.V) switch
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
						.Prepend(tools.Select(tool => (tool, ToolIconState.Normal)).ToHashMap())
						.DistinctUntilChanged()
						.ToVar(d);

				stateMap.Subscribe(_ => Invalidate()).D(d);

				this.Events().Click
					.Subscribe(_ =>
					{
						var tool = tools.FirstOrOption(tool => IconMap.Tool2IconR[tool].Contains(mousePos.V));
						tool.IfSome(setCurTool);
					}).D(d);


				this.Events().Paint.Subscribe(e =>
				{
					var gfx = e.Graphics;
					var r = new Rectangle(0, 0, ClientSize.Width, ClientSize.Height);
					gfx.FillRectangle(PaintUtils.BackBrush, r);

					foreach (var tool in tools)
					{
						var iconR = IconMap.Tool2IconR[tool];
						var state = stateMap.V[tool];
						var bmp = IconMap.State2Icon[(tool, state)];
						gfx.DrawImage(bmp, iconR.Min.X, iconR.Min.Y);
					}
				}).D(d);
			});
		}

	}
}

file static class PaintUtils
{
	public static readonly Brush BackBrush = new SolidBrush(MkCol(0x353535));
}