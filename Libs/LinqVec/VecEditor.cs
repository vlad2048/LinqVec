﻿using System.Reactive.Disposables;
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


public partial class VecEditor<TDoc> : UserControl
{
	private readonly ISubject<VecEditorInitNfo<TDoc>> whenInit;
	private IObservable<VecEditorInitNfo<TDoc>> WhenInit => whenInit.AsObservable();

	public ToolEnv<TDoc> Env { get; private set; } = null!;
	public void Init(VecEditorInitNfo<TDoc> initNfo)
	{
		whenInit.OnNext(initNfo);
		whenInit.OnCompleted();
	}

	public VecEditor()
	{
		InitializeComponent();
		var ctrlD = this.GetD();
		var transform = Var.Make(Transform.Id, ctrlD);
		whenInit = new AsyncSubject<VecEditorInitNfo<TDoc>>().D(ctrlD);
		var ctrl = new Ctrl(drawPanel);


		this.InitRX(WhenInit, (init, d) =>
		{
			var (doc, tools) = init;
			var (curTool, setCurTool, editorEvt) = VecEditorUtils.TrackUserEventsAndCurTool(drawPanel, tools, d);

			var tempD = MkD().D(ctrlD);
			var isPanZoom = PanZoomer.Setup(editorEvt, ctrl, transform, tempD);
			//var isPanZoom = PanZoomer.Setup(editorEvt, ctrl, transform, d);

			Env = new ToolEnv<TDoc>(
				doc,
				curTool,
				setCurTool,
				drawPanel,
				ctrl,
				isPanZoom,
				transform,
				editorEvt
			).D(d);

			var res = new Res().D(d);
			drawPanel.Init(new DrawPanelInitNfo(transform, res));
			if (DesignMode)
			{
				VecEditorUtils.SetupDesignMode(drawPanel, d);
				return;
			}

			doc.WhenUndoRedo.Subscribe(_ => Env.TriggerUndoRedo()).D(d);

			editorEvt.WhenKeyDown(Keys.D1).Subscribe(_ => Cursor = Cursors.Default).D(d);
			editorEvt.WhenKeyDown(Keys.D2).Subscribe(_ => Cursor = CBase.Cursors.Pen).D(d);
			editorEvt.WhenKeyDown(Keys.D3).Subscribe(_ => Cursor = CBase.Cursors.BlackArrowSmall).D(d);

			VecEditorUtils.RunTools(curTool, Env, d);

			var isMouseDown = editorEvt.IsMouseDown();
			//editorEvt.WhenKeyRepeat(Keys.Z, true).Where(_ => !isMouseDown.V).Subscribe(_ => doc.Undo()).D(d);
			//editorEvt.WhenKeyRepeat(Keys.Y, true).Where(_ => !isMouseDown.V).Subscribe(_ => doc.Redo()).D(d);
			editorEvt.WhenKeyDown(Keys.Z, true).Where(_ => !isMouseDown.V).Subscribe(_ => doc.Undo()).D(d);
			editorEvt.WhenKeyDown(Keys.Y, true).Where(_ => !isMouseDown.V).Subscribe(_ => doc.Redo()).D(d);

			statusStrip.AddLabel("panzoom", isPanZoom).D(d);
			statusStrip.AddLabel("zoom", transform.Select(e => $"{C.ZoomLevels[e.ZoomIndex]:P}")).D(d);
			statusStrip.AddLabel("center", transform.Select(e => e.Center)).D(d);
			statusStrip.AddLabel("tool", curTool.Select(GetToolName)).D(d);

			doc.WhenPaintNeeded.Subscribe(_ =>
			{
				drawPanel.Invalidate();
			}).D(d);

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
		var serDisp = new SerDisp().D(d);

		curTool
			.Subscribe(tool =>
			{
				var toolD = serDisp.GetNewD();

				var resetDisp = new SerDisp().D(toolD);
				void Reset()
				{
					var resetD = resetDisp.GetNewD();
					var toolActions = new ToolActions(
						Reset
					);
					tool.Run(env, toolActions).D(resetD);
				}
				Reset();
			}).D(d);
	}


	public static IDisposable Log<TDoc>(this IRoVar<ITool<TDoc>> curTool) =>
		curTool
			.Select(e => $"Tool <- {e.GetType().Name.RemoveSuffixIFP("Tool")}")
			.Subscribe(e => L.WriteLine(e, 0xfd5c5b));
}
