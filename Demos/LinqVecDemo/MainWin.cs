using System.Reactive.Linq;
using LinqVecDemo.Logic;
using PowRxVar;
using UILib;
using VectorEditor.Panes;
using WeifenLuo.WinFormsUI.Docking;

namespace LinqVecDemo;

partial class MainWin : Form
{
	public string? LastLoadedFile { get; set; }

	static MainWin()
	{
		WinFormsUtils.Tracker.Configure<MainWin>()
			.Id(e => e.Name, SystemInformation.VirtualScreen.Size)
			.Properties(e => new
			{
				e.LastLoadedFile
			})
			.PersistOn(nameof(Form.Move), nameof(Form.Resize), nameof(Form.FormClosing))
			.StopTrackingOn(nameof(Form.FormClosing));
	}

	public MainWin()
	{
		InitializeComponent();

		this.InitRX(this.Events().Load.ToUnit(), (_, d) =>
		{
			var doc = this.InitDocLogic().D(d);

			var modelTreePane = new ModelTreePane();
			modelTreePane.Show(dockPanel, DockState.DockRight);
			modelTreePane.Init(doc.Select(md => md.Select(f => f.ModelMan)));
		});
	}
}