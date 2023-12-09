using System.Reactive.Linq;
using System.Reactive.Subjects;
using LinqVec.Logic;
using LinqVec.Utils.WinForms_;
using PowMaybe;
using VectorEditor.Model;
using WeifenLuo.WinFormsUI.Docking;
using PowRxVar;
using UILib;
using VectorEditor.Panes.ModelTree_;

namespace VectorEditor.Panes;

public partial class ModelTreePane : DockContent
{
	private readonly ISubject<IObservable<Maybe<ModelMan<DocModel>>>> whenInit;
	private IObservable<IObservable<Maybe<ModelMan<DocModel>>>> WhenInit => whenInit.AsObservable();

	public void Init(IObservable<Maybe<ModelMan<DocModel>>> modelMan)
	{
		whenInit.OnNext(modelMan);
		whenInit.OnCompleted();
	}

	public ModelTreePane()
	{
		InitializeComponent();
		whenInit = new AsyncSubject<IObservable<Maybe<ModelMan<DocModel>>>>().D(this);

		this.InitRX(d =>
		{
			WhenInit.Subscribe(whenMayDoc =>
			{
				whenMayDoc.SubscribeWithDisp((mayDoc, docD) =>
				{
					modelTree.Clear();
					trackedList.Clear();
					if (mayDoc.IsSome(out var doc))
					{
						ModelTreeLogic.Setup(modelTree, doc).D(docD);
						ModelTrackedLogic.Setup(trackedList, doc).D(docD);
					}
				}).D(d);
			}).D(d);
		});
	}
}