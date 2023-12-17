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
using PowRxVar;

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

		var transform = Var.Make(Transform.Id).D(this);
		whenInit = new AsyncSubject<VecEditorInitNfo>().D(this);
		var curTool = Var.Make<ITool>(null!).D(this);
		var ctrl = new Ctrl(drawPanel);

		var editorEvt = EvtMaker.MakeForControl(drawPanel, curTool.ToUnitExt());
		var tempD = MkD().D(this);
		var isPanZoom = PanZoomer.Setup(editorEvt, ctrl, transform, tempD);

		Env = new ToolEnv(
			drawPanel,
			ctrl,
			curTool,
			isPanZoom,
			transform,
			editorEvt
		).D(this);


		this.InitRX(WhenInit, (init, d) =>
		{
			var res = new Res().D(d);
			drawPanel.Init(new DrawPanelInitNfo(transform, res));
			if (DesignMode) return;

			var (docUndoer, tools) = init;
			var undoMan = new UndoMan(docUndoer).D(d);
			Obs.Merge(undoMan.WhenUndo, undoMan.WhenRedo).Subscribe(_ => Env.SigUndoRedo()).D(d);

			editorEvt.WhenKeyDown(Keys.D1).Subscribe(_ => Cursor = Cursors.Default).D(d);
			editorEvt.WhenKeyDown(Keys.D2).Subscribe(_ => Cursor = CBase.Cursors.Pen).D(d);
			editorEvt.WhenKeyDown(Keys.D3).Subscribe(_ => Cursor = CBase.Cursors.BlackArrowSmall).D(d);

			Env.RunTools(tools, curTool, undoMan).D(d);

			editorEvt.WhenKeyRepeat(Keys.Z, true).Subscribe(_ => undoMan.Undo()).D(d);
			editorEvt.WhenKeyRepeat(Keys.Y, true).Subscribe(_ => undoMan.Redo()).D(d);

			statusStrip.AddLabel("panzoom", isPanZoom).D(d);
			statusStrip.AddLabel("zoom", transform.Select(e => $"{C.ZoomLevels[e.ZoomIndex]:P}")).D(d);
			statusStrip.AddLabel("center", transform.Select(e => e.Center)).D(d);
			statusStrip.AddLabel("tool", curTool.Select(GetToolName)).D(d);

			undoMan.WhenChanged.Subscribe(_ =>
			{
				Env.Invalidate();
			}).D(d);

			G.Cfg.RunWhen(e => e.Log.Tools, curTool.Log).D(d);
		});
	}

	private static string GetToolName(ITool tool) => tool.GetType().Name[..^4];
}




file static class VecEditorUtils
{
	public static IDisposable RunTools(this ToolEnv env, ITool[] tools, IRwVar<ITool> curTool, UndoMan undoMan)
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
						Reset,
						undoMan.SetToolUndoer
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
