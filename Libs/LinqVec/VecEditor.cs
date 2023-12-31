using System.Reactive.Linq;
using System.Reactive.Subjects;
using LinqVec.Controls;
using LinqVec.Structs;
using LinqVec.Utils.WinForms_;
using LinqVec.Logic;
using LinqVec.Utils;
using LinqVec.Tools;
using LinqVec.Tools.Events;
using UILib;
using LinqVec.Tools.Events.Utils;
using ReactiveVars;

namespace LinqVec;


public partial class VecEditor : UserControl
{
	public ToolEnv Env { get; }
	public EditorLogic Logic { get; }

	public VecEditor(EditorLogicMaker maker, Option<string> file)
	{
		var ctrlD = this.GetD();
		var transform = Var.Make(Transform.Id, ctrlD);

		InitializeComponent(transform);

		var ctrl = new Ctrl(drawPanel);

		var curTool = EmptyTool.Instance.Make(ctrlD);
		var (repeatLastMouseMove, whenRepeatLastMouseMove) = RxEventMaker.Make(ctrlD);
		var (toolReset, whenToolReset) = RxEventMaker.Make(ctrlD);
		var editorEvt = EvtMaker.MakeForControl(drawPanel, whenRepeatLastMouseMove);

		var isPanZoom = PanZoomer.Setup(editorEvt, ctrl, transform, ctrlD);

		Env = new ToolEnv(
			ctrl,
			curTool,
			isPanZoom,
			transform,
			editorEvt,
			toolReset
		).D(ctrlD);

		Logic = maker.Make(file, Env, ctrlD);

		VecEditorUtils.TrackCurTool(curTool, editorEvt, Logic.Tools, repeatLastMouseMove, ctrlD);


		this.InitRX(d =>
		{
			if (DesignMode)
			{
				VecEditorUtils.SetupDesignMode(drawPanel, d);
				return;
			}

			Logic.DocHolder.WhenUndoRedo.Subscribe(_ => Env.TriggerUndoRedo()).D(d);

			VecEditorUtils.RunTools(curTool, Env, whenToolReset, d);

			var isMouseDown = editorEvt.IsMouseDown();
			editorEvt.WhenKeyDown(Keys.Z, true).Where(_ => !isMouseDown.V).Subscribe(_ => Logic.DocHolder.Undo()).D(d);
			editorEvt.WhenKeyDown(Keys.Y, true).Where(_ => !isMouseDown.V).Subscribe(_ => Logic.DocHolder.Redo()).D(d);

			statusStrip.AddLabel("panzoom", isPanZoom).D(d);
			statusStrip.AddLabel("zoom", transform.Select(e => $"{C.ZoomLevels[e.ZoomIndex]:P}")).D(d);
			statusStrip.AddLabel("center", transform.Select(e => e.Center)).D(d);
			statusStrip.AddLabel("tool", curTool.Select(GetToolName)).D(d);

			Obs.Merge(
					Logic.DocHolder.WhenPaintNeeded,
					transform.ToUnit()
				)
				.Subscribe(_ => drawPanel.Invalidate()).D(d);

			G.Cfg.RunWhen(e => e.Log.CurTool, d, curTool.Log);
		});
	}

	private static string GetToolName(ITool tool) => tool.GetType().Name[..^4];
}




file static class VecEditorUtils
{
	public static void TrackCurTool(
		IRwVar<ITool> curTool,
		IObservable<IEvt> editorEvt,
		ITool[] tools,
		Action repeatLastMouseMove,
		Disp d
	)
	{
		tools
			.Select(tool =>
				editorEvt.WhenKeyDown(tool.Nfo.Shortcut).Select(_ => tool)
			)
			.Merge()
			.Subscribe(e => curTool.V = e).D(d);

		curTool.Subscribe(_ => repeatLastMouseMove()).D(d);
	}

	/*public static (
		IRoVar<ITool>,
		Action<ITool>,
		IObservable<IEvt>
	) TrackUserEventsAndCurTool(
		DrawPanel drawPanel,
		ITool[] tools,
		Disp d
	)
	{
		var (setCurTool, whenSetCurTool) = RxEventMaker.Make<ITool>(d);
		var curTool = TrackCurTool(editorEvt, tools, whenSetCurTool, d);
		curTool.Subscribe(_ => repeatLastMouseMove()).D(d);
		return (
			curTool,
			setCurTool,
			editorEvt
		);
	}


	private static IRoVar<ITool> TrackCurTool(
		IObservable<IEvt> whenEvt,
		ITool[] tools,
		IObservable<ITool> whenSetCurTool,
		Disp d
	) =>
		Obs.Merge(
				tools
					.Select(tool =>
						whenEvt.WhenKeyDown(tool.Shortcut).Select(_ => tool)
					)
					.Merge(),
				whenSetCurTool
			)
			.Prepend(EmptyTool.Instance)
			.ToVar(d);*/


	public static void RunTools(
		IRoVar<ITool> curTool,
		ToolEnv env,
		IObservable<Unit> whenToolReset,
		Disp d
	)
	{
		var serDisp = new SequentialSerialDisposable().D(d);

		curTool
			.DupWhen(whenToolReset)
			.Subscribe(tool =>
			{
				serDisp.DisposableFun = () =>
				{
					var toolD = MkD();
					tool.Run(toolD);
					return toolD;
				};
			}).D(d);
	}


	public static IDisposable Log(this IRoVar<ITool> curTool) =>
		curTool
			.Select(e => $"Tool <- {e.GetType().Name.RemoveSuffixIFP("Tool")}")
			.Subscribe(e => LC.WriteLine(e, 0xfd5c5b));


	private static readonly Brush designModeBackBrush = new SolidBrush(MkCol(0x542a57));

	public static void SetupDesignMode(DrawPanel drawPanel, Disp d)
	{
		drawPanel.Events().Paint.Subscribe(e =>
		{
			var gfx = e.Graphics;
			gfx.FillRectangle(designModeBackBrush, drawPanel.ClientRectangle);
		}).D(d);
	}
}
