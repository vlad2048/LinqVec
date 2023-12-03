using LinqVec.Logic;
using LinqVec.Tools.None_;
using LinqVec.Utils.WinForms_;
using PhysicsEditor.Tools.Play_;
using VectorEditor.Model;
using VectorEditor.Tools.Curve_;
using PowRxVar;

namespace LinqVecDemo;

sealed partial class MainWin : Form
{
	public MainWin()
	{
		InitializeComponent();

		this.InitRX(d =>
		{
			//var model = new Undoer<DocModel>(DocModel.Empty, vecEditor.EditorEvt).D(d);

			vecEditor.InitTools(
				new NoneTool(),
				new CurveTool()
				//new PlayTool()
			);
		});
	}
}