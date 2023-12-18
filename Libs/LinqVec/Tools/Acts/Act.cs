using Geom;
using LinqVec.Logic;
using LinqVec.Tools.Acts.Enums;

namespace LinqVec.Tools.Acts;


public static class Act
{
	public static HotspotActs Do<H>(
		this Hotspot<H> hotspot,
		Func<H, HotspotAct[]> actFuns
	) => new HotspotActs<H>(
		hotspot,
		actFuns
	).ToNonGeneric();


	public static HotspotAct Click(
		string name,
		ClickGesture gesture,
		Func<ActSetMaker> confirm
	) => new(
		name,
		(Gesture)gesture,
		new HotspotActActions(
			_ => { },
			_ => confirm()
		)
	);


	public static HotspotAct Drag<O>(
		string name,
		IMouseModder<O> modder,
		Func<Pt, MouseMod<O>> mod
	) => new(
		name,
		Gesture.Drag,
		new HotspotActActions(
			DragStart: p => modder.ModSet(mod(p)),
			Confirm: p =>
			{
				modder.ModApply(p);
				return None;
			}
		)
	);
}