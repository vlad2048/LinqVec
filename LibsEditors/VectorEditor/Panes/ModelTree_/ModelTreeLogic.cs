using BrightIdeasSoftware;
using LinqVec.Logic;
using LinqVec.Utils.WinForms_;
using PowRxVar;
using VectorEditor.Model;

namespace VectorEditor.Panes.ModelTree_;

static class ModelTreeLogic
{
	public static IDisposable Setup(TreeListView ctrl, ModelMan<DocModel> modelMan)
	{
		var d = new Disp();

		/*SetNods(ctrl);
		ctrl.AddTextColumn<CurveModel>("Curve", null, e => $"pts:{e.Pts.Length}");

		modelMan.WhenChanged
			.ObserveOnUI()
			.Subscribe(_ =>
			{
				ctrl.SetObjects(modelMan.V.Curves);
			}).D(d);*/

		return d;
	}

	private static void SetNods(TreeListView ctrl)
	{
		ctrl.CanExpandGetter = _ => false;
		ctrl.ChildrenGetter = _ => Array.Empty<object>();
		ctrl.ParentGetter = _ => null;
	}

	static void AddTextColumn<T>(
		this ObjectListView ctrl,
		string name,
		int? width,
		Func<T, string> textFun
	) =>
		ctrl.Columns.Add(new OLVColumn(name, name)
		{
			Width = width ?? 60,
			FillsFreeSpace = !width.HasValue,
			AspectGetter = obj => obj switch
			{
				T nod => textFun(nod),
				_ => null
			}
		});
}