using System.Reactive.Linq;
using Geom;
using LinqVec.Tools.Cmds.Enums;
using PtrLib;
using ReactiveVars;

namespace LinqVec.Tools.Cmds;


public static class Cmd
{
	public static Hotspot<TH> OnHover<TH>(this Hotspot<TH> hotspot, Func<IRoVar<Option<Pt>>, IDisposable> hoverAction) => hotspot with { HoverAction = hoverAction };

	public static ClickHotspotCmd Click(
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