using System.Reactive.Disposables;
using System.Reactive.Linq;
using Geom;
using LinqVec.Tools.Cmds.Enums;
using PtrLib;
using ReactiveVars;

namespace LinqVec.Tools.Cmds;


public static class Cmd
{
	public static Hotspot<TH> OnHover<TH>(this Hotspot<TH> hotspot, Func<IRoVar<Option<Pt>>, IDisposable> hoverAction) => hotspot with { HoverAction = hoverAction };

	public static readonly Func<IRoVar<Option<Pt>>, IDisposable> EmptyHoverAction = _ => Disposable.Empty;

	public static ClickHotspotCmd ClickRet(
		string name,
		ClickGesture gesture,
		Func<Option<ToolStateFun>> clickAction
	) => new(
		name,
		(Gesture)gesture,
		clickAction
	);

	public static DragHotspotCmd Drag(
		string name,
		Func<Pt, IRoVar<Option<Pt>>, IDisposable> dragAction
	) => new(
		name,
		Gesture.Drag,
		dragAction
	);


	// *************
	// * Utilities *
	// *************
	public static ClickHotspotCmd Click(
		string name,
		ClickGesture gesture,
		Action clickAction
	) => new(
		name,
		(Gesture)gesture,
		() =>
		{
			clickAction();
			return None;
		}
	);
}


public static class CmdModExt
{
	public static Func<Pt, IRoVar<Option<Pt>>, IDisposable> ModSetDrag<T>(this IPtrRegular<T> ptr, string name, Func<Pt, Pt, T, T> fun) =>
		(ptStart, ptEnd) => ptr.ModSet(new Mod<T>(
			name,
			true,
			ptEnd
				.Select(ptEndOpt => ptEndOpt.Match(
					ptEndV => Mk<T>(ptrV => fun(ptStart, ptEndV, ptrV)),
					() => Mk<T>(ptrV => ptrV)
				))
				.ToVar()
		));

	public static Func<IRoVar<Option<Pt>>, IDisposable> ModSetHover<T>(this IPtrRegular<T> ptr, string name, Func<Pt, T, T> fun) =>
		pt => ptr.ModSet(new Mod<T>(
			name,
			false,
			pt
				.Select(ptOpt => ptOpt.Match(
					ptV => Mk<T>(ptrV => fun(ptV, ptrV)),
					() => Mk<T>(ptrV => ptrV)
				))
				.ToVar()
		));

	private static Func<T, T> Mk<T>(Func<T, T> f) => f;
}


public static class GizmoExt
{
	public static Func<IRoVar<Option<Pt>>, IDisposable> UpdateGizmo<Gizmo>(
		this Func<IRoVar<Option<Pt>>, IDisposable> dragHover,
		Action<Func<Gizmo, Gizmo>> applyFun,
		Func<Gizmo, Gizmo>? funStart = null,
		Func<Gizmo, Gizmo>? funEnd = null
	) =>
		pt =>
		{
			funStart.Apply(applyFun);
			var dragActionD = dragHover(pt);
			return Disposable.Create(() =>
			{
				funEnd.Apply(applyFun);
				dragActionD.Dispose();
			});
		};



	public static Func<IRoVar<Option<Pt>>, IDisposable> UpdateGizmoTemp<Gizmo>(
		this Func<IRoVar<Option<Pt>>, IDisposable> dragHover,
		Action<Func<Gizmo, Gizmo>> applyFun,
		Func<Gizmo, Gizmo> funStart
	) =>
		pt =>
		{
			var prev = funStart.ApplyAndGetPreviousValue(applyFun);
			Func<Gizmo, Gizmo> funEnd = _ => prev;
			var dragActionD = dragHover(pt);
			return Disposable.Create(() =>
			{
				funEnd.Apply(applyFun);
				dragActionD.Dispose();
			});
		};





	public static Func<Pt, IRoVar<Option<Pt>>, IDisposable> UpdateGizmo<Gizmo>(
		this Func<Pt, IRoVar<Option<Pt>>, IDisposable> dragAction,
		Action<Func<Gizmo, Gizmo>> applyFun,
		Func<Gizmo, Gizmo>? funStart = null,
		Func<Gizmo, Gizmo>? funEnd = null
	) =>
		(ptStart, ptEnd) =>
		{
			funStart.Apply(applyFun);
			var dragActionD = dragAction(ptStart, ptEnd);
			return Disposable.Create(() =>
			{
				funEnd.Apply(applyFun);
				dragActionD.Dispose();
			});
		};



	public static Func<Pt, IRoVar<Option<Pt>>, IDisposable> UpdateGizmoTemp<Gizmo>(
		this Func<Pt, IRoVar<Option<Pt>>, IDisposable> dragAction,
		Action<Func<Gizmo, Gizmo>> applyFun,
		Func<Gizmo, Gizmo> funStart
	) =>
		(ptStart, ptEnd) =>
		{
			var prev = funStart.ApplyAndGetPreviousValue(applyFun);
			Func<Gizmo, Gizmo> funEnd = _ => prev;
			var dragActionD = dragAction(ptStart, ptEnd);
			return Disposable.Create(() =>
			{
				funEnd.Apply(applyFun);
				dragActionD.Dispose();
			});
		};



	private static void Apply<Gizmo>(this Func<Gizmo, Gizmo>? fun, Action<Func<Gizmo, Gizmo>> applyFun)
	{
		if (fun == null) return;
		applyFun(fun);
	}

	private static Gizmo ApplyAndGetPreviousValue<Gizmo>(this Func<Gizmo, Gizmo> fun, Action<Func<Gizmo, Gizmo>> applyFun)
	{
		Gizmo prev = default!;
		applyFun(e =>
		{
			prev = e;
			return fun(e);
		});
		return prev;
	}
}