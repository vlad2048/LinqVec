using System.Reactive.Linq;
using LinqVec;
using LinqVec.Panes;
using LinqVec.Tools;
using LinqVec.Utils;
using PowBasics.CollectionsExt;
using ReactiveVars;
using WeifenLuo.WinFormsUI.Docking;

namespace LinqVecDemo.Logic;

static class PanesLogic
{
	private sealed record Ctx(
		EditorLogicMaker Maker,
		DockPanel DockPanel,
		IRoVar<Option<DocPane>> Doc,
		Disp D
	);

	public static void InitPanesLogic(
		this MainWin win,
		EditorLogicMaker maker,
		IRoVar<Option<DocPane>> doc,
		Disp d
	)
	{
		var ctx = new Ctx(maker, win.dockPanel, doc, d);

		if (ctx.Maker.Caps.HasFlag(EditorLogicCaps.SupportLayoutPane))
			HookPane<LayoutPane>(ctx, MakeLayoutPane, win.menuViewLayout, DockState.DockRight, true);
		else
			win.menuViewLayout.Visible = false;

		HookPane<ToolsPane>(ctx, MakeToolsPane, win.menuViewTools, DockState.DockLeft, true);
	}


	private static void HookPane<T>(Ctx ctx, Func<Ctx, DockContent> fun, ToolStripMenuItem menuItem, DockState dockState, bool showOnStart) where T : DockContent
	{
		var isDisplayed = Var.Make(false, ctx.D);
		ctx.DockPanel.WhenActiveContentChanged().Subscribe(_ => isDisplayed.V = IsPaneDisplayed<T>(ctx.DockPanel)).D(ctx.D);

		void Open() => fun(ctx).Show(ctx.DockPanel, dockState);
		void Close() => ctx.DockPanel.Contents.OfType<T>().ToArray().ForEach(e => e.DockHandler.DockPanel = null);

		isDisplayed.Subscribe(on => menuItem.Checked = on).D(ctx.D);
		menuItem.Events().Click.Subscribe(_ =>
		{
			isDisplayed.V = IsPaneDisplayed<T>(ctx.DockPanel);
			if (isDisplayed.V)
				Close();
			else
				Open();
			isDisplayed.V = IsPaneDisplayed<T>(ctx.DockPanel);
		}).D(ctx.D);

		if (showOnStart)
		{
			Open();
			isDisplayed.V = IsPaneDisplayed<T>(ctx.DockPanel);
		}
	}


	private static DockContent MakeLayoutPane(Ctx ctx)
	{
		var pane = new LayoutPane();
		ctx.Doc.WhereNone().Subscribe(_ => pane.layoutTree.ClearObjects()).D(ctx.D);
		ctx.Doc.WhereSome().Subscribe(docV => docV.Logic.SetupLayoutPane(pane.layoutTree, ctx.D)).D(ctx.D);
		return pane;
	}
	private static DockContent MakeToolsPane(Ctx ctx)
	{
		var toolSet = ctx.Doc.GetToolSet();
		var (curTool, setCurTool) = ctx.Doc.GetCurTool();
		var pane = new ToolsPane(toolSet, curTool, setCurTool);
		return pane;
	}



	private static bool IsPaneDisplayed<T>(DockPanel dockPanel) => dockPanel.Contents.OfType<T>().ToArray().Any();

	private static IObservable<Unit> WhenActiveContentChanged(this DockPanel dockPanel) => Obs.FromEventPattern(e => dockPanel.ActiveContentChanged += e, e => dockPanel.ActiveContentChanged -= e).ToUnit();
}




file static class DocExt
{
	public static IRoVar<ITool[]> GetToolSet(this IRoVar<Option<DocPane>> doc) =>
		doc
			.Select(e => e.Match(
				f => f.Logic.Tools,
				() => [EmptyTool.Instance]
			))
			.ToVar();

	public static (IRoVar<ITool>, Action<ITool>) GetCurTool(this IRoVar<Option<DocPane>> doc)
	{
		var curTool =
			doc
				.Select(e => e.Match(
					f => f.Env.CurTool.AsObservable(),
					() => Obs.Return(EmptyTool.Instance)
				))
				.Switch()
				.ToVar();

		void setCurTool(ITool v) => doc.V.IfSome(e => e.Env.CurTool.V = v);

		return (curTool, setCurTool);
	}
}