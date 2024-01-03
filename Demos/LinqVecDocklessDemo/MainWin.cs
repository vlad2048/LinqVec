using VectorEditor;

namespace LinqVecDocklessDemo;

partial class MainWin : Form
{
	public MainWin()
	{
		InitializeComponent(VectorEditorLogicMaker.Instance, None);
	}
}