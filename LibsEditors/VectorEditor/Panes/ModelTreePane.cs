using System.Reactive.Linq;
using System.Reactive.Subjects;
using LinqVec.Logic;
using PowMaybe;
using VectorEditor.Model;
using WeifenLuo.WinFormsUI.Docking;
using PowRxVar;
using UILib;
using VectorEditor.Panes.ModelTree_;

namespace VectorEditor.Panes;

public partial class ModelTreePane : DockContent
{
	private readonly ISubject<IObservable<Maybe<Model<Doc>>>> whenInit;
	private IObservable<IObservable<Maybe<Model<Doc>>>> WhenInit => whenInit.AsObservable();

	public void Init(IObservable<Maybe<Model<Doc>>> modelMan)
	{
		whenInit.OnNext(modelMan);
		whenInit.OnCompleted();
	}

	public ModelTreePane()
	{
		InitializeComponent();
		whenInit = new AsyncSubject<IObservable<Maybe<Model<Doc>>>>().D(this);

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
						//ModelTrackedLogic.Setup(trackedList, doc).D(docD);
					}
				}).D(d);
			}).D(d);
		});
	}
}