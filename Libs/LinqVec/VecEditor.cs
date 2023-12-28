using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using LinqVec.Controls;
using LinqVec.Drawing;
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

namespace LinqVec;


public partial class VecEditor : UserControl
{
	private readonly ISubject<VecEditorInitNfo> whenInit;
	private IObservable<VecEditorInitNfo> WhenInit => whenInit.AsObservable();

	public ToolEnv Env { get; }
	public void Init(VecEditorInitNfo initNfo)
	{
		whenInit.OnNext(initNfo);
		whenInit.OnCompleted();
	}

	public VecEditor()
	{
		InitializeComponent();

		var ctrlD = this.GetD();
		var transform = Var.Make(Transform.Id, ctrlD);
		whenInit = new AsyncSubject<VecEditorInitNfo>().D(ctrlD);
		var curTool = Var.Make<ITool>(null!, ctrlD);
		var ctrl = new Ctrl(drawPanel);

		var editorEvt = EvtMaker.MakeForControl(drawPanel, curTool.ToUnit());
		var tempD = MkD().D(ctrlD);
		var isPanZoom = PanZoomer.Setup(editorEvt, ctrl, transform, tempD);

		Env = new ToolEnv(
			drawPanel,
			ctrl,
			curTool,
			isPanZoom,
			transform,
			editorEvt
		).D(ctrlD);


		this.InitRX(WhenInit, (init, d) =>
		{
			var res = new Res().D(d);
			drawPanel.Init(new DrawPanelInitNfo(transform, res));
			if (DesignMode) return;

			var (model, tools) = init;
			model.WhenUndoRedo.Subscribe(_ => Env.TriggerUndoRedo()).D(d);

			editorEvt.WhenKeyDown(Keys.D1).Subscribe(_ => Cursor = Cursors.Default).D(d);
			editorEvt.WhenKeyDown(Keys.D2).Subscribe(_ => Cursor = CBase.Cursors.Pen).D(d);
			editorEvt.WhenKeyDown(Keys.D3).Subscribe(_ => Cursor = CBase.Cursors.BlackArrowSmall).D(d);

			Env.RunTools(tools, curTool).D(d);

			var isMouseDown = editorEvt.IsMouseDown();
			editorEvt.WhenKeyRepeat(Keys.Z, true).Where(_ => !isMouseDown.V).Subscribe(_ => model.Undo()).D(d);
			editorEvt.WhenKeyRepeat(Keys.Y, true).Where(_ => !isMouseDown.V).Subscribe(_ => model.Redo()).D(d);

			statusStrip.AddLabel("panzoom", isPanZoom).D(d);
			statusStrip.AddLabel("zoom", transform.Select(e => $"{C.ZoomLevels[e.ZoomIndex]:P}")).D(d);
			statusStrip.AddLabel("center", transform.Select(e => e.Center)).D(d);
			statusStrip.AddLabel("tool", curTool.Select(GetToolName)).D(d);

			model.WhenPaintNeeded.Subscribe(_ =>
			{
				drawPanel.Invalidate();
			}).D(d);

			G.Cfg.RunWhen(e => e.Log.CurTool, d, curTool.Log);
		});
	}

	private static string GetToolName(ITool tool) => tool.GetType().Name[..^4];
}




file static class VecEditorUtils
{
	public static IDisposable RunTools(this ToolEnv env, ITool[] tools, IRwVar<ITool> curTool)
	{
		var d = MkD();

		var serDisp = new SerDisp().D(d);

		tools
			.Select(tool =>
				env.EditorEvt.WhenKeyDown(tool.Shortcut).Select(_ => tool)
			)
			.Merge()
			.Prepend(tools[0])
			.Subscribe(tool =>
			{
				var toolD = serDisp.GetNewD();
				curTool.V = tool;

				var resetDisp = new SerDisp().D(toolD);
				void Reset()
				{
					var resetD = resetDisp.GetNewD();
					var toolActions = new ToolActions(
						Reset
					);
					tool.Run(toolActions).D(resetD);
				}
				Reset();
			}).D(d);

		return d;
	}

	public static IDisposable Log(this IRoVar<ITool> curTool) =>
		curTool
			.Select(e => $"Tool <- {e.GetType().Name.RemoveSuffixIFP("Tool")}")
			.Subscribe(e => L.WriteLine(e, 0xfd5c5b));
}
