using BrightIdeasSoftware;
using DynamicData;
using LinqVec;
using LinqVec.Logic;
using LinqVec.Utils.WinForms_;
using PowBasics.CollectionsExt;
using ReactiveVars;
using UILib;
using UILib.Utils;
using VectorEditor.Model;

namespace VectorEditor.Panes.ModelTree_;


/*static class ModelTrackedLogic
{
	public static IDisposable Setup(ObjectListView list, ModelMan<DocModel> mm)
	{
		var d = MkD();

		list.SetupColumns();

		var s = new SourceCache<int, int>(e => e);

		mm.WhenChanged
			//.ObserveOnUI()
			.Subscribe(_ =>
			{
				var tracked = mm.GetTracked().SelectToArray(e => Nod.Make(new DocNode(e)));
				list.SetObjects(tracked);
			}).D(d);

		return d;
	}
}*/


static class ModelTreeLogic
{
	public static IDisposable Setup(TreeListView tree, IUndoerReadOnly<Doc> doc)
	{
		var d = MkD();

		tree.SetNodGeneric<DocNode>();

		tree.SetupColumns();

		doc.CurReadOnly
			//.ObserveOnUI()
			.Subscribe(cur =>
			{
				var roots = cur.ToTree();
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
		list.AddTextColumn<TNod<DocNode>>("name", null, nod => nod.V.Obj switch
		{
			Layer => "layer",
			Curve => "curve",
			_ => "unknown"
		});

		list.AddTextColumn<TNod<DocNode>>("info", null, nod => nod.V.Obj switch
		{
			Layer e => $"kids:{e.Objects.Length}",
			Curve e => $"points:{e.Pts.Length}",
			_ => "unknown"
		});

		list.AddTextColumn<TNod<DocNode>>("id", 70, nod => $"{nod.V.Obj.Id}");
	}
}
