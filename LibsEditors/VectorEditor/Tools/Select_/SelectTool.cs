using LinqVec;
using LinqVec.Logic;
using LinqVec.Structs;
using LinqVec.Tools;
using LinqVec.Tools.Acts;
using LinqVec.Tools.Enums;
using LinqVec.Tools.Events;
using PowMaybe;
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
			.TrackPos(out var mousePos, d)
			.MakeHot(d)
			.ToEvt(e => Env.Curs.Cursor = e);

		var sel = VarMay.Make<IVisualObjSer>().D(d);

		Act.Loop(
				Act.Amb(

					Act.Make(
						"Select",
						Hotspots.Object(Doc),
						Trigger.Down,
						null,
						onHover: mayObj => Env.Curs.Cursor = mayObj.IsSome() ? CBase.Cursors.BlackArrowHold : CBase.Cursors.BlackArrow,
						onTrigger: obj => sel.V = May.Some(obj)
					),

					Act.Make(
						"Deselect",
						Hotspots.Anywhere,
						Trigger.Down,
						null,
						onHover: null,
						onTrigger: _ => sel.V = May.None<IVisualObjSer>()
					)

				)
			)
			.Run(evt).D(d);

		Env.WhenPaint.Subscribe(gfx =>
		{
			if (sel.V.IsSome(out var obj))
			{
				SelectPainter.DrawSelRect(gfx, obj);
			}
		}).D(d);

		sel.Subscribe(_ =>
		{
			Env.Invalidate();
		}).D(d);

		return (Undoer.Empty, d);
	}
}



static class Hotspots
{
	public static Func<Pt, Maybe<Pt>> Anywhere => May.Some;
	public static Func<Pt, Maybe<IVisualObjSer>> Object(Model<Doc> mm) => mm.V.GetObjectAt;
}