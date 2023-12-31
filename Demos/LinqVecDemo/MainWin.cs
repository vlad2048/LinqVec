using LinqVec;
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
			var editorLogic = new VectorEditorLogic();
			var doc = this.InitDocLogic(editorLogic, d);
			this.InitPanesLogic(editorLogic, doc, d);
		});
	}
}