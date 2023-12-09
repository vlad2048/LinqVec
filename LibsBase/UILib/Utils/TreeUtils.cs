using BrightIdeasSoftware;

namespace UILib.Utils;

public static class TreeUtils
{
	public static void SetNodGeneric<T>(this TreeListView ctrl)
	{
		ctrl.CanExpandGetter = delegate (object o)
		{
			var nod = (TNod<T>)o;
			return nod.Children.Any();
		};
		ctrl.ChildrenGetter = delegate (object o)
		{
			var nod = (TNod<T>)o;
			return nod.Children;
		};
		ctrl.ParentGetter = delegate (object o)
		{
			var nod = (TNod<T>)o;
			return nod.Parent;
		};
	}

	public static void AddTextColumn<T>(
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