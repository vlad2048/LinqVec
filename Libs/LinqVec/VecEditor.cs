using System.Reactive.Disposables;
using System.Reactive.Linq;
using PowRxVar;
using LinqVec.Controls;
using LinqVec.Drawing;
using LinqVec.Structs;
using LinqVec.Utils.WinForms_;
using LinqVec.Logic;
using LinqVec.Utils;
using LinqVec.Tools._Base;
using LinqVec.Tools._Base.Events;
using LinqVec.Tools.None_;

namespace LinqVec;

public partial class VecEditor : UserControl
{
	private static readonly Tool[] emptyTools = { new NoneTool() };

	private Tool[] tools = emptyTools;
	private bool AreToolsInited => tools != emptyTools;
	private readonly IRwVar<Tool> curTool;
	private readonly IToolEnv env;

	public IObservable<IEvtGen<PtInt>> EditorEvt { get; }

	public VecEditor()
	{
		InitializeComponent();

		var transform = Var.Make(Transform.Id).D(this);
		curTool = Var.Make(tools[0]).D(this);
		var ctrl = new Ctrl(drawPanel);

		EditorEvt = EvtUtils.MakeForControl(drawPanel, curTool.ToUnit());
		var isPanZoom = PanZoomer.Setup(EditorEvt, ctrl, transform).D(this);

		env = new ToolEnv(drawPanel, ctrl, curTool, isPanZoom, transform, EditorEvt);

		/*drawPanel.Events().KeyPress.Subscribe(e =>
		{
			L.WriteLine($"keyc:{e.KeyChar}  ctrl:{KeyUtils.IsCtrlPressed}");
		}).D(this);*/

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

		tools
			.Select(tool =>
				drawPanel.Events().KeyDown
					.Where(e => e.KeyCode == tool.Shortcut)
					.Select(_ => tool)
			)
			.Merge()
			.SubscribeWithDisp((tool, lifeD) =>
			{
				Disposable.Create(() => env.Invalidate()).D(lifeD);
				curTool.V = tool.Init(env).D(lifeD);
				env.Invalidate();
			}).D(this);
	}
}