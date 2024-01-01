using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Geom;
using LinqVec.Tools.Cmds.Enums;
using LinqVec.Utils.Rx;
using PtrLib;
using ReactiveVars;

namespace LinqVec.Tools.Cmds;


public static class Cmd
{
	public static Hotspot<TH> OnHover<TH>(this Hotspot<TH> hotspot, Func<IRoVar<Option<Pt>>, Action<bool>> hoverAction) => hotspot with { HoverAction = hoverAction };

	public static readonly Func<IRoVar<Option<Pt>>, Action<bool>> EmptyHoverAction = _ => commit => {};

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
		Func<Pt, IRoVar<Option<Pt>>, Action<bool>> dragAction
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
	public static Func<Pt, IRoVar<Option<Pt>>, Action<bool>> ModSetDrag<T>(this IScopedPtr<T> ptr, string name, Func<Pt, Pt, T, T> fun) =>
		(ptStart, ptEnd) =>
		{
			var (source, action) = ptEnd
				.Select(ptEndOpt => ptEndOpt.Match(
					ptEndV => Mk<T>(ptrV => fun(ptStart, ptEndV, ptrV)),
					() => Mk<T>(ptrV => ptrV)
				))
				.ToVar()
				.TerminateWithAction();
			ptr.SetMod(new Mod<T>(name, source));
			return action;
		};

	public static Func<IRoVar<Option<Pt>>, Action<bool>> ModSetHover<T>(this IScopedPtr<T> ptr, string name, Func<Pt, T, T> fun) =>
		pt =>
		{
			var (source, action) = pt
				.Select(ptOpt => ptOpt.Match(
					ptV => Mk<T>(ptrV => fun(ptV, ptrV)),
					() => Mk<T>(ptrV => ptrV)
				))
				.ToVar()
				.TerminateWithAction();
			ptr.SetMod(new Mod<T>(name, source));
			return action;
		};


	/*
	public static Func<Pt, IRoVar<Option<Pt>>, IDisposable> ModSetDrag<T>(this IScopedPtr<T> ptr, string name, Func<Pt, Pt, T, T> fun) =>
		(ptStart, ptEnd) => ptr.SetMod(new Mod<T>(
			name,
			ptEnd
				.Select(ptEndOpt => ptEndOpt.Match(
					ptEndV => Mk<T>(ptrV => fun(ptStart, ptEndV, ptrV)),
					() => Mk<T>(ptrV => ptrV)
				))
				.ToVar()
		));

	public static Func<IRoVar<Option<Pt>>, IDisposable> ModSetHover<T>(this IScopedPtr<T> ptr, string name, Func<Pt, T, T> fun) =>
		pt => ptr.SetMod(new Mod<T>(
			name,
			pt
				.Select(ptOpt => ptOpt.Match(
					ptV => Mk<T>(ptrV => fun(ptV, ptrV)),
					() => Mk<T>(ptrV => ptrV)
				))
				.ToVar()
		));
	*/

	private static Func<T, T> Mk<T>(Func<T, T> f) => f;
}


public static class GizmoExt
{
	public static Func<IRoVar<Option<Pt>>, Action<bool>> UpdateGizmo<Gizmo>(
		this Func<IRoVar<Option<Pt>>, Action<bool>> dragHover,
		Action<Func<Gizmo, Gizmo>> applyFun,
		Func<Gizmo, Gizmo>? funStart = null,
		Func<Gizmo, Gizmo>? funEnd = null
	) =>
		pt => mf(() => dragHover(pt)).UpdateInternal(applyFun, funStart, funEnd, End.Set)();



	public static Func<IRoVar<Option<Pt>>, Action<bool>> UpdateGizmoTemp<Gizmo>(
		this Func<IRoVar<Option<Pt>>, Action<bool>> dragHover,
		Action<Func<Gizmo, Gizmo>> applyFun,
		Func<Gizmo, Gizmo> funStart
	) =>
		pt => mf(() => dragHover(pt)).UpdateInternal(applyFun, funStart, null, End.Restore)();


	public static Func<Pt, IRoVar<Option<Pt>>, Action<bool>> UpdateGizmo<Gizmo>(
		this Func<Pt, IRoVar<Option<Pt>>, Action<bool>> dragAction,
		Action<Func<Gizmo, Gizmo>> applyFun,
		Func<Gizmo, Gizmo>? funStart = null,
		Func<Gizmo, Gizmo>? funEnd = null
	) =>
		(ptStart, ptEnd) => mf(() => dragAction(ptStart, ptEnd)).UpdateInternal(applyFun, funStart, funEnd, End.Set)();


	public static Func<Pt, IRoVar<Option<Pt>>, Action<bool>> UpdateGizmoTemp<Gizmo>(
		this Func<Pt, IRoVar<Option<Pt>>, Action<bool>> dragAction,
		Action<Func<Gizmo, Gizmo>> applyFun,
		Func<Gizmo, Gizmo> funStart
	) =>
		(ptStart, ptEnd) => mf(() => dragAction(ptStart, ptEnd)).UpdateInternal(applyFun, funStart, null, End.Restore)();


	private static Func<Action<bool>> mf(Func<Action<bool>> f) => f;

	private enum End
	{
		Set,
		Leave,
		Restore,
	}

	private static Func<Action<bool>> UpdateInternal<Gizmo>(
		this Func<Action<bool>> dragAction,
		Action<Func<Gizmo, Gizmo>> applyFun,
		Func<Gizmo, Gizmo>? funStart,
		Func<Gizmo, Gizmo>? funEnd,
		End end
	) =>
		() =>
		{
			var gizmoPrev = applyFun.ApplyAndGetPreviousValue(funStart);
			return commitPrev =>
			{
				switch (end)
				{
					case End.Set:
						applyFun.Apply(funEnd);
						break;
					case End.Leave:
						break;
					case End.Restore:
						applyFun.Apply(_ => gizmoPrev);
						break;
				}
				dragAction()(commitPrev);
			};
		};



	private static void Apply<Gizmo>(this Action<Func<Gizmo, Gizmo>> applyFun, Func<Gizmo, Gizmo>? fun)
	{
		fun ??= e => e;
		applyFun(fun);
	}

	private static Gizmo ApplyAndGetPreviousValue<Gizmo>(this Action<Func<Gizmo, Gizmo>> applyFun, Func<Gizmo, Gizmo>? fun)
	{
		fun ??= e => e;
		Gizmo prev = default!;
		applyFun(e =>
		{
			prev = e;
			return fun(e);
		});
		return prev;
	}
}