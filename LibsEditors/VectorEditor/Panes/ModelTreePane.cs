using System.Reactive.Linq;
using System.Reactive.Subjects;
using LinqVec.Logic;
using LinqVec.Utils.WinForms_;
using VectorEditor.Model;
using WeifenLuo.WinFormsUI.Docking;
using PowRxVar;
using VectorEditor.Panes.ModelTree_;

namespace VectorEditor.Panes;

public partial class ModelTreePane : DockContent
{
	private readonly ISubject<ModelMan<DocModel>> whenInit;
	private IObservable<ModelMan<DocModel>> WhenInit => whenInit.AsObservable();

	public void Init(ModelMan<DocModel> modelMan)
	{
		whenInit.OnNext(modelMan);
		whenInit.OnCompleted();
	}

	public ModelTreePane()
	{
		InitializeComponent();
		whenInit = new AsyncSubject<ModelMan<DocModel>>().D(this);

		this.InitRX(d =>
		{
			WhenInit.Subscribe(modelMan =>
			{
				ModelTreeLogic.Setup(modelTree, modelMan).D(d);
			}).D(d);
		});
	}
}