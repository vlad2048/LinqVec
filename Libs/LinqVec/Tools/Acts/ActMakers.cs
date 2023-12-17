using Geom;
using LinqVec.Logic;
using LinqVec.Tools.Acts.Delegates;
using LinqVec.Tools.Acts.Enums;
using LinqVec.Tools.Acts.Structs;

namespace LinqVec.Tools.Acts;


public static class Act
{
	public static ActNfo Click<H>(
		string id,
		ClickGesture gesture,
		Hotspot<H> hotspot,
		Func<H, ActMaker> confirm
	) => Click(
		id,
		gesture,
		hotspot,
		h => Option<ActMaker>.Some(confirm(h))
	);


	public static ActNfo Click<H>(
		string id,
		ClickGesture gesture,
		Hotspot<H> hotspot,
		Func<H, Option<ActMaker>> confirm
	) => new(
		id,
		(Gesture)gesture,
		hotspot.ToNonGeneric(),
		new ActActions<H>(
			(_, _) => {},
			() => {},
			(_, _) => {},
			(e, _) => confirm(e)
			//confirmActs
		).ToNonGeneric()
	);

	public static ActNfo DragMod<O, H>(
		string id,
		Hotspot<H> hotspot,
		IMouseModder<O> modder,
		MouseModStartHot<O, H> mod,
		bool applyOnHover
	) => new(
		id,
		Gesture.Drag,
		hotspot.ToNonGeneric(),
		new ActActions<H>(
			HoverOn: (h, p) =>
			{
				if (applyOnHover)
					modder.ModSet(mod(p, h));
			},
			HoverOff: () =>
			{
				if (applyOnHover)
					modder.ModClear();
			},
			DragStart: (h, p) => modder.ModSet(mod(p, h)),
			Confirm: (_, p) =>
			{
				modder.ModApply(p);
				return Option<ActMaker>.None;
			}
			//ConfirmActs: null
		).ToNonGeneric()
	);

	public static ActNfo DragMod<O>(
		string id,
		Hotspot<Pt> hotspot,
		IMouseModder<O> modder,
		MouseModStart<O> mod,
		bool applyOnHover
	) => new(
		id,
		Gesture.Drag,
		hotspot.ToNonGeneric(),
		new ActActions<Pt>(
			HoverOn: (_, p) =>
			{
				if (applyOnHover)
					modder.ModSet(mod(p));
			},
			HoverOff: () =>
			{
				if (applyOnHover)
					modder.ModClear();
			},
			DragStart: (_, p) => modder.ModSet(mod(p)),
			Confirm: (_, p) =>
			{
				modder.ModApply(p);
				return Option<ActMaker>.None;
			}
		//ConfirmActs: null
		).ToNonGeneric()
	);
}
