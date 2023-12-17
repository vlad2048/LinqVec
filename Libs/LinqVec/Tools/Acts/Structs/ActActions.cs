using Geom;
using LinqVec.Tools.Acts.Delegates;

namespace LinqVec.Tools.Acts.Structs;

public sealed record ActActions(
	Action<object, Pt> HoverOn,
	Action HoverOff,
	Action<object, Pt> DragStart,
	Action<object, Pt> Confirm,
	ActMaker? ConfirmActs
);


public sealed record ActActions<H>(
	Action<H, Pt> HoverOn,
	Action HoverOff,
	Action<H, Pt> DragStart,
	Action<H, Pt> Confirm,
	ActMaker<H>? ConfirmActs
);


static class ActActionsExt
{
	public static ActActions ToNonGeneric<H>(this ActActions<H> actions) => new(
		(h, p) => actions.HoverOn((H)h, p),
		actions.HoverOff,
		(h, p) => actions.DragStart((H)h, p),
		(h, p) => actions.Confirm((H)h, p),
		actions.ConfirmActs.ToNonGeneric()
	);
}