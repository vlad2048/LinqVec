using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using PowRxVar;
using LinqVec.Controls;
using LinqVec.Drawing;
using LinqVec.Structs;
using LinqVec.Utils.WinForms_;
using LinqVec.Logic;
using LinqVec.Utils;
using LinqVec.Tools;
using LinqVec.Tools.Events;
using LinqVec.Utils.Rx;

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

		var noneTool = new NoneTool(() => Cursor = Cursors.Default);
		var transform = Var.Make(Transform.Id).D(this);
		whenInit = new AsyncSubject<VecEditorInitNfo>().D(this);
		var curTool = Var.Make<ITool>(noneTool).D(this);
		var ctrl = new Ctrl(drawPanel);

		var editorEvt = EvtUtils.MakeForControl(drawPanel, curTool.ToUnit());
		var isPanZoom = PanZoomer.Setup(editorEvt, ctrl, transform).D(this);

		Env = new ToolEnv(
			drawPanel,
			ctrl,
			curTool,
			isPanZoom,
			transform,
			editorEvt
		);


		this.InitRX(WhenInit, (init, d) =>
		{
			var res = new Res().D(d);
			drawPanel.Init(new DrawPanelInitNfo(transform, res));
			if (DesignMode) return;

			var (whenToolResetRequired, tools) = init;

			var toolRunner = Env.RunTools(
				tools.Append(noneTool).ToArray(),
				curTool
			).D(d);

			whenToolResetRequired.Subscribe(_ => toolRunner.Reset()).D(d);

			statusStrip.AddLabel("panzoom", isPanZoom).D(d);
			statusStrip.AddLabel("zoom", transform.Select(e => $"{C.ZoomLevels[e.ZoomIndex]:P}")).D(d);
			statusStrip.AddLabel("center", transform.Select(e => e.Center)).D(d);
			statusStrip.AddLabel("tool", Var.Expr(() => $"{curTool.V.Name}{(toolRunner.IsAtRest.V ? " (rest)" : "")}")).D(this);
		});
	}
}



interface IToolRunner
{
	IRoVar<bool> IsAtRest { get; }
	void Reset();
}

file static class VecEditorUtils
{
	public static (IToolRunner, IDisposable) RunTools(this ToolEnv env, ITool[] tools, IRwVar<ITool> curTool)
	{
		var d = new Disp();

		var isAtRest = Var.Make(true).D(d);
		var cur = new SerVar<ToolRun>(new ToolRun(curTool.V, e => isAtRest.V = e)).D(d);

		cur.SelectVar(e => e.Tool).PipeTo(curTool);

		tools
			.Select(tool =>
				env.EditorEvt.WhenKeyDown(tool.Shortcut)
					.Select(_ => tool)
			)
			.Merge()
			.Subscribe(tool =>
			{
				cur.V = new ToolRun(tool, e =>
				{
					isAtRest.V = e;
				});
			}).D(d);

		var toolRunner = new ToolRunner(
			() => cur.V.Reset(),
			isAtRest
		);

		return (toolRunner, d);
	}

	private sealed class ToolRunner : IToolRunner
	{
		private readonly Action reset;

		public ToolRunner(Action reset, IRoVar<bool> isAtRest)
		{
			this.reset = reset;
			IsAtRest = isAtRest;
		}

		public void Reset() => reset();
		public IRoVar<bool> IsAtRest { get; }
	}


	private sealed class ToolRun : IDisposable
	{
		private interface IToolState;
		private sealed record RestToolState : IToolState;
		private sealed record RunToolState(Pt StartPt) : IToolState;

		private readonly Disp d = new();
		public void Dispose() => d.Dispose();

		private readonly Action<bool> setIsAtRest;
		private readonly ISubject<Pt> whenStart;
		private readonly SerialDisp<IDisposable> stateD;
		private IToolState state = null!;

		private IObservable<Pt> WhenStart => whenStart.AsObservable();

		private IToolState State
		{
			get => state;
			set
			{
				if (value == state) return;
				state = value;
				setIsAtRest(state is RestToolState);
				stateD.Value = null;
				stateD.Value = state switch
				{
					RestToolState => Tool.RunRest(whenStart.OnNext),
					RunToolState { StartPt: var startPt } => Tool.Run(startPt),
					_ => throw new ArgumentException()
				};
			}
		}

		public ITool Tool { get; }
		public void Reset() => State = new RestToolState();

		public ToolRun(ITool tool, Action<bool> setIsAtRest)
		{
			this.setIsAtRest = setIsAtRest;
			Tool = tool;
			whenStart = new Subject<Pt>().D(d);
			stateD = new SerialDisp<IDisposable>().D(d);
			State = new RestToolState();

			WhenStart.Subscribe(startPt => State = new RunToolState(startPt)).D(d);
		}
	}
}














/*public partial class VecEditor : UserControl
{
	//private static readonly Tool[] emptyTools = { new NoneTool() };
	//private bool AreToolsInited => tools != emptyTools;
	//private readonly IRwVar<Tool> curTool;
	//private readonly ISubject<Tool> whenOverrideTool;
	//private IObservable<VecEditorInitNfo> WhenInit => whenInit.AsObservable();
	//private IObservable<Tool> WhenOverrideTool => whenOverrideTool.AsObservable();

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
		whenOverrideTool = new Subject<Tool>().D(this);
		curTool = VarMay.Make(tools[0]).D(this);
		var ctrl = new Ctrl(drawPanel);

		var editorEvt = EvtUtils.MakeForControl(drawPanel, curTool.ToUnit());
		var isPanZoom = PanZoomer.Setup(editorEvt, ctrl, transform).D(this);

		Env = new ToolEnv(drawPanel, ctrl, curTool, isPanZoom, transform, editorEvt, () => whenOverrideTool.OnNext(tools[0]));


		this.InitRX(d =>
		{
			var res = new Res().D(d);
			drawPanel.Init(new DrawPanelInitNfo(transform, res));
			if (DesignMode) return;

			statusStrip.AddLabel("panzoom", isPanZoom).D(d);
			statusStrip.AddLabel("zoom", transform.Select(e => $"{C.ZoomLevels[e.ZoomIndex]:P}")).D(d);
			statusStrip.AddLabel("center", transform.Select(e => e.Center)).D(d);

			WhenInit.Subscribe(init =>
			{
				var tools = init.Tools;
			}).D(d);
		});
	}

	public void InitTools(params Tool[] pTools)
	{
		if (AreToolsInited) throw new ArgumentException("Tools already inited");
		tools = pTools;
		if (!AreToolsInited) throw new ArgumentException("Tools not inited correctly");
		InitToolsFinish();
	}

	private void InitToolsFinish()
	{
		curTool.V = tools[0];
		statusStrip.AddLabel("tool", curTool.Select(e => e.Name)).D(this);

		Obs.Merge(
				tools
					.Select(tool =>
						drawPanel.Events().KeyDown
							.Where(e => e.KeyCode == tool.Shortcut)
							.Select(_ => tool)
					)
					.Merge(),
				WhenOverrideTool
			)
			.SubscribeWithDisp((tool, lifeD) =>
			{
				Disposable.Create(() => Env.Invalidate()).D(lifeD);
				curTool.V = tool.Init(Env).D(lifeD);
				Env.Invalidate();
			}).D(this);
	}
}*/