using System.Reactive.Linq;
using LinqVec;
using LinqVec.Panes;
using LinqVec.Tools;
using LinqVec.Utils.Rx;
using PowBasics.CollectionsExt;
using ReactiveVars;
using WeifenLuo.WinFormsUI.Docking;

namespace LinqVecDemo.Logic;

static class PanesLogic
{
	public static void InitPanesLogic(this MainWin win, IRoVar<Option<DocPane<TDoc>>> doc, Disp d)
	{
		var editorLogic = LogicSelector.Instance;
		win.menuViewLayout.ShowShortcutKeys = true;

		// LayoutPane
		// ==========
		if (editorLogic.Caps.HasFlag(EditorLogicCaps.SupportLayoutPane))
			HookPane<LayoutPane>(win.dockPanel, DockState.DockRight, true, win.menuViewLayout, doc, MakeLayoutPane, d);
		else
			win.menuViewLayout.Visible = false;

		// ToolsPane
		// =========
		HookPane<ToolsPane<TDoc>>(win.dockPanel, DockState.DockLeft, true, win.menuViewTools, doc, MakeToolsPane, d);
	}

	private static void HookPane<T>(
		DockPanel dockPanel,
		DockState dockState,
		bool showOnStart,
		ToolStripMenuItem menuItem,
		IRoVar<Option<DocPane<TDoc>>> doc,
		Action<DockPanel, DockState, IRoVar<Option<DocPane<TDoc>>>, Disp> makeFun,
		Disp d
	) where T : DockContent
	{
		var isDisplayed = Var.Make(false, d);
		dockPanel.WhenActiveContentChanged().Subscribe(_ => isDisplayed.V = IsPaneDisplayed<T>(dockPanel)).D(d);

		isDisplayed.Subscribe(on => menuItem.Checked = on).D(d);
		menuItem.Events().Click.Subscribe(_ =>
		{
			isDisplayed.V = IsPaneDisplayed<T>(dockPanel);
			TogglePane<T>(dockPanel, isDisplayed, () => makeFun(dockPanel, dockState, doc, d));
			isDisplayed.V = IsPaneDisplayed<T>(dockPanel);
		}).D(d);

		if (showOnStart)
		{
			TogglePane<T>(dockPanel, isDisplayed, () => makeFun(dockPanel, dockState, doc, d));
			isDisplayed.V = IsPaneDisplayed<T>(dockPanel);
		}
	}

	private static void TogglePane<T>(DockPanel dockPanel, IRoVar<bool> isDisplayed, Action makeFun) where T : DockContent
	{
		if (isDisplayed.V)
			dockPanel.Contents.OfType<T>().ToArray().ForEach(e => e.DockHandler.DockPanel = null);
		else
			makeFun();
	}

	private static void MakeLayoutPane(DockPanel dockPanel, DockState dockState, IRoVar<Option<DocPane<TDoc>>> doc, Disp d)
	{
		var editorLogic = LogicSelector.Instance;
		var layoutPane = new LayoutPane();
		layoutPane.Show(dockPanel, dockState);

		var docObs =
			doc
				.Select(e => e.Match(
					f => f.Doc.CurReadOnly.Select(Some),
					() => Obs.Return(Option<TDoc>.None)
				))
				.Switch();

		editorLogic.SetupLayoutPane(layoutPane.layoutTree, docObs, d);
	}


	private static void MakeToolsPane(DockPanel dockPanel, DockState dockState, IRoVar<Option<DocPane<TDoc>>> doc, Disp d)
	{
		var editorLogic = LogicSelector.Instance;
		var (curTool, setCurTool) = doc.GetCurTool(d);
		var toolsPane = new ToolsPane<TDoc>(editorLogic.Tools, curTool, setCurTool);
		toolsPane.Width = 64;
		toolsPane.Show(dockPanel, dockState);
	}







	/*private static IRoVar<bool> IsPaneDisplayed<T>(DockPanel dockPanel, Disp d) where T : DockContent
	{
		var isDisplayed = Var.Make(false, d);
		dockPanel.WhenActiveContentChanged().Subscribe(_ => isDisplayed.V = IsPaneDisplayed<T>(dockPanel)).D(d);
		return isDisplayed;
	}*/

	private static bool IsPaneDisplayed<T>(DockPanel dockPanel) => dockPanel.Contents.OfType<T>().ToArray().Any();

	private static IObservable<Unit> WhenActiveContentChanged(this DockPanel dockPanel) => Obs.FromEventPattern(e => dockPanel.ActiveContentChanged += e, e => dockPanel.ActiveContentChanged -= e).ToUnit();
}




file static class DocExt
{
	public static (IRoVar<ITool<TDoc>>, Action<ITool<TDoc>>) GetCurTool(this IRoVar<Option<DocPane<TDoc>>> doc, Disp d)
	{
		var curTool =
			doc
				.Select(e => e.Match(
					f => f.vecEditor.Env.CurTool.AsObservable(),
					() => Obs.Return(EmptyTool<TDoc>.Instance)
				))
				.Switch()
				.ToVar(d);

		void setCurTool(ITool<TDoc> v) => doc.V.IfSome(e => e.vecEditor.Env.SetCurTool(v));

		return (curTool, setCurTool);
	}




	/*
		return new FunRwVar<ITool<TDoc>>(
			obs => cur.V.Match(
				e => e.Subscribe(obs),
				() => Disposable.Empty
			),
			() => cur.V.Match(
				e => e.V,
				() => emptyTool
			),
			v => cur.V.Match(
				e => e.V = v,
				() => {}
			),
			() => cur.V.Match(
				e => e.IsDisposed,
				() => false
			)
		);
	}


	private static readonly ITool<TDoc> emptyTool = new EmptyTool();

	private sealed class EmptyTool : ITool<TDoc>
	{
		public string Name => "_";
		public Bitmap? Icon => null;
		public Keys Shortcut => 0;
		public Disp Run(ToolEnv<TDoc> Env, ToolActions toolActions) => MkD();
	}



	private sealed class FunRwVar<T>(
		Func<IObserver<T>, IDisposable> subscribeFun,
		Func<T> getFun,
		Action<T> setFun,
		Func<bool> isDisposedFun
	) : IRwVar<T>
	{
		public IDisposable Subscribe(IObserver<T> observer) => subscribeFun(observer);
		public T V
		{
			get => getFun();
			set => setFun(value);
		}
		public bool IsDisposed => isDisposedFun();
	}
	*/
}