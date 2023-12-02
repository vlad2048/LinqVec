namespace LinqVecDemo;

sealed partial class MainWin : Form
{
	public MainWin()
	{
		InitializeComponent();

		vecEditor.InitTools(
			new NoneTool(),
			new CurveTool()
			//new SnapTool()
		);
	}
}