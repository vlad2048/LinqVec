using LinqVec;
using LinqVec.Tools;
using LinqVec.Utils.Rx;
using LinqVecDemo.Logic;
using ReactiveVars;
using UILib;
using VectorEditor;

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
			.PersistOn(nameof(Move), nameof(Resize), nameof(FormClosing))
			.StopTrackingOn(nameof(FormClosing));
	}

	public MainWin()
	{
		InitializeComponent();

		this.InitRX(this.Events().Load.ToUnit(), (_, d) =>
		{
			var doc = this.InitDocLogic(VectorEditorLogicMaker.Instance, d);
			this.InitPanesLogic(VectorEditorLogicMaker.Instance, doc, d);
		});
	}
}