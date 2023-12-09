using LinqVec;
using PowRxVar;
using LinqVec.Logic;
using LinqVec.Structs;
using VectorEditor.Model;
using LinqVec.Tools;
using LinqVec.Tools.Acts;
using LinqVec.Tools.Events;
using PowMaybe;
using VectorEditor.Tools.Curve_.Mods;
using VectorEditor.Tools.Curve_.Structs;
using LinqVec.Tools.Enums;

namespace VectorEditor.Tools.Curve_;


sealed class CurveTool(ToolEnv env, ModelMan<DocModel> mm) : Tool<DocModel>(env, mm)
{
	public override Keys Shortcut => Keys.P;

	public override IDisposable Run(Action reset)
	{
		var d = new Disp();

		var evt = Env.GetEvtForTool(this)
			.ToGrid(Env.Transform)
			.SnapToGrid(Env.Transform)
			.TrackPos(out var mousePos, d)
			.RestrictToGrid()
			.MakeHot(d)
			.ToEvt(e => Env.Curs.Cursor = e);

		var curve = mm.Create(Entities.Curve(mm.V.Layers[0].Id)).D(d);
		var gfxState = CurveGfxState.None;

		Action<Maybe<T>> SetState<T>(CurveGfxState state) => mp =>
		{
			if (mp.IsSome())
				gfxState = state;
		};

		evt.WhenKeyDown(Keys.Escape).Subscribe(_ =>
		{
			reset();
		}).D(d);

		evt.WhenKeyDown(Keys.Enter).Subscribe(_ =>
		{
			curve.Commit();
			reset();
		}).D(d);


		Act.Loop(
				Act.Amb(

					Act.Seq(
						Act.Make(
							"Move point",
							Hotspots.CurvePoint(curve),
							Trigger.Down,
							CBase.Cursors.BlackArrowSmall,
							onHover: SetState<PointId>(CurveGfxState.None),
							onTrigger: pointId => curve.ModSet(CurveMods.MovePoint(pointId))
						),
						Act.Make(
							"Move point (finish)",
							Hotspots.Anywhere,
							Trigger.Up,
							CBase.Cursors.BlackArrowSmall,
							onHover: null,
							onTrigger: m => curve.ModApply(m)
						)
					),

					Act.Seq(
						Act.Make(
							"Add point",
							Hotspots.Anywhere,
							Trigger.Down,
							CBase.Cursors.Pen,
							onHover: mp =>
							{
								SetState<Pt>(CurveGfxState.AddPoint)(mp);
								curve.ModSet(CurveMods.AddPoint(mp));
							},
							onTrigger: startPt => curve.ModSet(CurveMods.AddPoint(startPt))
						),
						Act.Make(
							"Add point (finish)",
							Hotspots.Anywhere,
							Trigger.Up,
							CBase.Cursors.Pen,
							onHover: SetState<Pt>(CurveGfxState.DragHandle),
							onTrigger: curve.ModApply
						)
					)

				)
			)
			.Run(evt).D(d);

		env.WhenPaint.Subscribe(gfx =>
		{
			CurvePainter.Draw(gfx, curve.ModGfxApply(mousePos.V), gfxState);
		}).D(d);

		return d;
	}
}


static class Hotspots
{
	public static Func<Pt, Maybe<Pt>> Anywhere => May.Some;
	public static Func<Pt, Maybe<PointId>> CurvePoint(IEntity<CurveModel> curve) => m => curve.V.GetClosestPointTo(m, C.ActivateMoveMouseDistance);
}
