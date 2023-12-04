using LinqVec.Utils.WinForms_;
using VectorEditor.Panes;
using WeifenLuo.WinFormsUI.Docking;

namespace LinqVecDemo;

partial class MainWin : Form
{
	public MainWin()
	{
		InitializeComponent();
		var mainPane = new MainPane();
		var modelTreePane = new ModelTreePane();
		mainPane.Show(dockPanel, DockState.Document);
		modelTreePane.Show(dockPanel, DockState.DockRight);


		this.Events().Load.Subscribe(_ =>
		{
			modelTreePane.Init(mainPane.ModelMan);
		}).D(this);
	}
}