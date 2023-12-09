using BrightIdeasSoftware;
using DynamicData;
using LinqVec.Logic;
using LinqVec.Utils.WinForms_;
using PowBasics.CollectionsExt;
using PowRxVar;
using UILib;
using UILib.Utils;
using VectorEditor.Model;

namespace VectorEditor.Panes.ModelTree_;

static class ModelTrackedLogic
{
	public static IDisposable Setup(ObjectListView list, ModelMan<DocModel> mm)
	{
		var d = new Disp();

		list.SetupColumns();

		var s = new SourceCache<int, int>(e => e);

		mm.WhenChanged
			.ObserveOnUI()
			.Subscribe(_ =>
			{
				var tracked = mm.GetTracked().SelectToArray(e => Nod.Make(new ModelNode(e)));
				list.SetObjects(tracked);
			}).D(d);

		return d;
	}
}


static class ModelTreeLogic
{
	public static IDisposable Setup(TreeListView tree, ModelMan<DocModel> mm)
	{
		var d = new Disp();

		tree.SetNodGeneric<ModelNode>();

		tree.SetupColumns();

		mm.WhenChanged
			.ObserveOnUI()
			.Subscribe(_ =>
			{
				var roots = mm.V.ToTree();
				tree.SetObjects(roots);
				tree.ExpandAll();
			}).D(d);

		return d;
	}
}


file static class CommonLogic
{
	public static void SetupColumns(this ObjectListView list)
	{
		list.AddTextColumn<TNod<ModelNode>>("name", null, nod => nod.V.Obj switch
		{
			LayerModel => "layer",
			CurveModel => "curve",
			_ => "unknown"
		});

		list.AddTextColumn<TNod<ModelNode>>("info", null, nod => nod.V.Obj switch
		{
			LayerModel e => $"kids:{e.Objects.Length}",
			CurveModel e => $"points:{e.Pts.Length}",
			_ => "unknown"
		});

		list.AddTextColumn<TNod<ModelNode>>("id", 70, nod => $"{nod.V.Obj.Id}");
	}
}