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

namespace LinqVec;

public partial class VecEditor : UserControl
{
	private static readonly Tool[] emptyTools = { new NoneTool() };

	private Tool[] tools = emptyTools;
	private bool AreToolsInited => tools != emptyTools;
	private readonly IRwVar<Tool> curTool;
	private readonly ISubject<Tool> whenOverrideTool;
	private IObservable<Tool> WhenOverrideTool => whenOverrideTool.AsObservable();

	public ToolEnv Env { get; }

	public VecEditor()
	{
		InitializeComponent();

		var transform = Var.Make(Transform.Id).D(this);
		whenOverrideTool = new Subject<Tool>().D(this);
		curTool = Var.Make(tools[0]).D(this);
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
}