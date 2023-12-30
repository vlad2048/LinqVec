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
using LinqVec.Utils.Rx;
using ReactiveVars;
using PtrLib;

namespace LinqVec;


public partial class VecEditor<TDoc> : UserControl
{
	public ToolEnv<TDoc> Env { get; }

	public VecEditor(IPtr<TDoc> doc, ITool<TDoc>[] tools)
	{
		var ctrlD = this.GetD();
		var transform = Var.Make(Transform.Id, ctrlD);

		InitializeComponent(transform);

		var ctrl = new Ctrl(drawPanel);
		var (curTool, setCurTool, editorEvt) = VecEditorUtils.TrackUserEventsAndCurTool(drawPanel, tools, ctrlD);

		var isPanZoom = PanZoomer.Setup(editorEvt, ctrl, transform, ctrlD);

		Env = new ToolEnv<TDoc>(
			doc,
			curTool,
			setCurTool,
			drawPanel,
			ctrl,
			isPanZoom,
			transform,
			editorEvt
		).D(ctrlD);


		this.InitRX(d =>
		{
			if (DesignMode)
			{
				VecEditorUtils.SetupDesignMode(drawPanel, d);
				return;
			}

			doc.WhenUndoRedo.Subscribe(_ => Env.TriggerUndoRedo()).D(d);

			/*editorEvt.WhenKeyDown(Keys.D1).ObserveOnUI().Subscribe(_ => LT.Log("KeyDown")).D(d);
			editorEvt.WhenKeyUp(Keys.D1).ObserveOnUI().Subscribe(_ => LT.Log("KeyUp")).D(d);
			editorEvt.WhenMouseMove().ObserveOnUI().Subscribe(_ => LT.Log("MouseMove")).D(d);
			editorEvt.WhenMouseDown().ObserveOnUI().Subscribe(_ => LT.Log("MouseDown")).D(d);
			editorEvt.WhenMouseUp().ObserveOnUI().Subscribe(_ => LT.Log("MouseUp")).D(d);
			editorEvt.WhenMouseWheel().ObserveOnUI().Subscribe(_ => LT.Log("MouseWheel")).D(d);*/


			VecEditorUtils.RunTools(curTool, Env, d);

			var isMouseDown = editorEvt.IsMouseDown();
			editorEvt.WhenKeyDown(Keys.Z, true).Where(_ => !isMouseDown.V).Subscribe(_ => doc.Undo()).D(d);
			editorEvt.WhenKeyDown(Keys.Y, true).Where(_ => !isMouseDown.V).Subscribe(_ => doc.Redo()).D(d);

			statusStrip.AddLabel("panzoom", isPanZoom).D(d);
			statusStrip.AddLabel("zoom", transform.Select(e => $"{C.ZoomLevels[e.ZoomIndex]:P}")).D(d);
			statusStrip.AddLabel("center", transform.Select(e => e.Center)).D(d);
			statusStrip.AddLabel("tool", curTool.Select(GetToolName)).D(d);

			Obs.Merge(
					doc.WhenPaintNeeded,
					transform.ToUnit()
				)
				.Subscribe(_ => drawPanel.Invalidate()).D(d);

			G.Cfg.RunWhen(e => e.Log.CurTool, d, curTool.Log);
		});
	}

	private static string GetToolName(ITool<TDoc> tool) => tool.GetType().Name[..^4];
}




file static class VecEditorUtils
{
	public static (
		IRoVar<ITool<TDoc>>,
		Action<ITool<TDoc>>,
		IObservable<IEvt>
	) TrackUserEventsAndCurTool<TDoc>(
		DrawPanel drawPanel,
		ITool<TDoc>[] tools,
		Disp d
	)
	{
		var (setCurTool, whenSetCurTool) = RxEventMaker.Make<ITool<TDoc>>(d);
		var (repeatLastMouseMove, whenRepeatLastMouseMove) = RxEventMaker.Make(d);
		var editorEvt = EvtMaker.MakeForControl(drawPanel, whenRepeatLastMouseMove);
		var curTool = TrackCurTool(editorEvt, tools, whenSetCurTool, d);
		curTool.Subscribe(_ => repeatLastMouseMove()).D(d);
		return (
			curTool,
			setCurTool,
			editorEvt
		);
	}


	private static IRoVar<ITool<TDoc>> TrackCurTool<TDoc>(
		IObservable<IEvt> whenEvt,
		ITool<TDoc>[] tools,
		IObservable<ITool<TDoc>> whenSetCurTool,
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
			.Prepend(EmptyTool<TDoc>.Instance)
			.ToVar(d);



	private static readonly Brush designModeBackBrush = new SolidBrush(MkCol(0x542a57));

	public static void SetupDesignMode(DrawPanel drawPanel, Disp d)
	{
		drawPanel.Events().Paint.Subscribe(e =>
		{
			var gfx = e.Graphics;
			gfx.FillRectangle(designModeBackBrush, drawPanel.ClientRectangle);
		}).D(d);
	}

	public static void RunTools<TDoc>(
		IRoVar<ITool<TDoc>> curTool,
		ToolEnv<TDoc> env,
		Disp d
	)
	{
		var serDisp = new SequentialSerialDisposable().D(d);
		ISubject<Unit> whenReset = new Subject<Unit>().D(d);
		IObservable<Unit> WhenReset = whenReset.AsObservable();

		curTool
			.DupWhen(WhenReset)
			.Subscribe(tool =>
			{
				var toolActions = new ToolActions(
					() => whenReset.OnNext(Unit.Default)
				);
				serDisp.DisposableFun = () => tool.Run(env, toolActions);
			}).D(d);
	}


	public static IDisposable Log<TDoc>(this IRoVar<ITool<TDoc>> curTool) =>
		curTool
			.Select(e => $"Tool <- {e.GetType().Name.RemoveSuffixIFP("Tool")}")
			.Subscribe(e => LC.WriteLine(e, 0xfd5c5b));
}
