using LinqVec.Tools.Curve_;
using LinqVec.Tools.None_;

namespace LinqVecDemo;

sealed partial class MainWin : Form
{
	public MainWin()
	{
		InitializeComponent();

		vecEditor.InitTools(
			new NoneTool(),
			new CurveTool()
		);
	}
}