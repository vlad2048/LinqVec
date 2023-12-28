using System.Reactive.Linq;
using LinqVec;
using LinqVec.Panes;
using LinqVec.Utils.Rx;
using ReactiveVars;
using UILib.Utils;
using WeifenLuo.WinFormsUI.Docking;

namespace LinqVecDemo.Logic;

static class PanesLogic
{
	public static void InitPanesLogic(this MainWin win, IRoVar<Option<DocPane<TDoc>>> doc, Disp d)
	{
		var editorLogic = LogicSelector.Instance;

		if (editorLogic.Caps.HasFlag(EditorLogicCaps.SupportLayoutPane))
		{
			var isDisplayed = IsPaneDisplayed<LayoutPane>(win.dockPanel, d);
			isDisplayed.Disables(d, win.menuViewLayout);
			win.menuViewLayout.Events().Click.Subscribe(_ =>
			{
				CreateLayoutPane(win.dockPanel, doc, d);
			}).D(d);
		}
		else
		{
			win.menuViewLayout.Visible = false;
		}
	}



	private static void CreateLayoutPane(DockPanel dockPanel, IRoVar<Option<DocPane<TDoc>>> doc, Disp d)
	{
		var editorLogic = LogicSelector.Instance;
		if (editorLogic.Caps.HasFlag(EditorLogicCaps.SupportLayoutPane))
		{
			var layoutPane = new LayoutPane();
			layoutPane.Show(dockPanel, DockState.DockRight);

			var docObs =
				doc
					.Select(e => e.Match(
						f => f.Doc.CurReadOnly.Select(Some),
						() => Obs.Return(Option<TDoc>.None)
					))
					.Switch();

			editorLogic.SetupLayoutPane(layoutPane.layoutTree, docObs, d);
		}
	}




	private static IRoVar<bool> IsPaneDisplayed<T>(DockPanel dockPanel, Disp d)
	{
		var isDisplayed = Var.Make(false, d);
		dockPanel.WhenActiveContentChanged().Subscribe(_ =>
		{
			var panes = dockPanel.Contents.OfType<T>().ToArray();
			isDisplayed.V = panes.Any();
		}).D(d);
		return isDisplayed;
	}

	private static IObservable<Unit> WhenActiveContentChanged(this DockPanel dockPanel) => Obs.FromEventPattern(e => dockPanel.ActiveContentChanged += e, e => dockPanel.ActiveContentChanged -= e).ToUnit();
}