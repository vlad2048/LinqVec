using Geom;
using LinqVec;
using LinqVec.Logic;
using LinqVec.Tools;
using LinqVec.Tools.Acts;
using LinqVec.Tools.Enums;
using LinqVec.Tools.Events;
using LinqVec.Tools.Events.Utils;
using LinqVec.Utils.Rx;
using PowRxVar;
using VectorEditor.Model;

namespace VectorEditor.Tools.Select_;



sealed class SelectTool(ToolEnv Env, Model<Doc> Doc) : ITool
{
	public Keys Shortcut => Keys.Q;

	public (IUndoer, IDisposable) Run(Action reset)
	{
		var d = new Disp();

		var evt = Env.GetEvtForTool(this)
			.ToGrid(Env.Transform)
			.SnapToGrid()
			.RestrictToGrid()
			.TrackMouse(out var mousePos, d)
			.MakeHot(d)
			.ToEvt(e => Env.Curs.Cursor = e);

		var maySel = new SerMay<IModder<IVisualObjSer>>().D(d);
		IModder<IVisualObjSer> MkMod(IVisualObjSer obj) => Mod.Doc(Entities.Visual(Doc, obj), mousePos);

		Act.Loop(
				Act.Amb(

					Act.Make(
						"Select",
						Hotspots.Object(Doc),
						Trigger.Down,
						null,
						onHover: mayObj => Env.Curs.Cursor = mayObj.IsSome ? CBase.Cursors.BlackArrowHold : CBase.Cursors.BlackArrow,
						onTrigger: curve =>
						{
							var mod = MkMod(curve);
							maySel.V = Some(mod);
							//mod.Mod
						}),

					Act.Make(
						"Deselect",
						Hotspots.Anywhere,
						Trigger.Down,
						null,
						onHover: null,
						onTrigger: _ => maySel.V = Option<IModder<IVisualObjSer>>.None
					)

				)
			)
			.Run(evt).D(d);

		Env.WhenPaint.Subscribe(gfx =>
		{
			if (maySel.V.IsSome)
			{
				var mod = maySel.V.IfNone(() => throw new ArgumentException());
				SelectPainter.DrawSelRect(gfx, mod.V);
			}
		}).D(d);

		maySel.WhenChanged.Subscribe(_ =>
		{
			Env.Invalidate();
		}).D(d);

		return (Undoer.Empty, d);
	}
}



static class Hotspots
{
	public static Func<Pt, Option<Pt>> Anywhere => Option<Pt>.Some;
	//public static Func<Pt, Maybe<IVisualObjSer>> Object(Model<Doc> mm) => pt => mm.V.GetObjectAt(pt).IsSome(out var obj) && obj is Curve curve ? May.Some(curve) : May.None<Curve>();
	public static Func<Pt, Option<IVisualObjSer>> Object(Model<Doc> mm) => mm.V.GetObjectAt;
}