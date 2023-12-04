using LinqVec.Logic;
using LinqVec.Utils.WinForms_;
using VectorEditor.Model;
using PowRxVar;
using VectorEditor;
using WeifenLuo.WinFormsUI.Docking;

namespace LinqVecDemo;

sealed partial class MainPane : DockContent
{
	public ModelMan<DocModel> ModelMan { get; private set; } = null!;

	public MainPane()
	{
		InitializeComponent();

		this.InitRX(d =>
		{
			ModelMan = vecEditor.InitVectorEditor().D(d);
		});
	}
}