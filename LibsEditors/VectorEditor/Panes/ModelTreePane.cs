using System.Reactive.Linq;
using System.Reactive.Subjects;
using LinqVec.Logic;
using VectorEditor.Model;
using WeifenLuo.WinFormsUI.Docking;
using UILib;
using VectorEditor.Panes.ModelTree_;
using PowRxVar;

namespace VectorEditor.Panes;

public partial class ModelTreePane : DockContent
{
	private readonly ISubject<IObservable<Option<Model<Doc>>>> whenInit;
	private IObservable<IObservable<Option<Model<Doc>>>> WhenInit => whenInit.AsObservable();

	public void Init(IObservable<Option<Model<Doc>>> modelMan)
	{
		whenInit.OnNext(modelMan);
		whenInit.OnCompleted();
	}

	public ModelTreePane()
	{
		InitializeComponent();
		whenInit = new AsyncSubject<IObservable<Option<Model<Doc>>>>().D(this);

		this.InitRX(d =>
		{
			WhenInit.Subscribe(whenMayDoc =>
			{
				var serDisp = new SerDisp().D(d);
				whenMayDoc.Subscribe(mayDoc =>
				{
					var docD = serDisp.GetNewD();
					modelTree.Clear();
					trackedList.Clear();
					if (mayDoc.IsSome)
					{
						var doc = mayDoc.IfNone(() => throw new ArgumentException());
						ModelTreeLogic.Setup(modelTree, doc).D(docD);
					}
				}).D(d);
			}).D(d);
		});
	}
}