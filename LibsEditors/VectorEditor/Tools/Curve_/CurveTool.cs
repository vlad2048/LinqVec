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
	public override Keys Shortcut => Keys.F1;

	public override IDisposable Run()
	{
		var d = new Disp();

		var evt = Env.GetEvtForTool(this)
			.ToGrid(Env.Transform)
			.SnapToGrid(Env.Transform)
			.TrackPos(out var mousePos, d)
			.RestrictToGrid()
			.MakeHot(d)
			.ToEvt(e => Env.Curs.Cursor = e);

		var curve = mm.Create(Entities.Curve(mm.V.Layers[0].Id));

		


		Act.Loop(
				Act.Amb(

					Act.Seq(
						Act.Make(
							"Move point",
							Hotspots.CurvePoint(curve),
							Trigger.Down,
							CBase.Cursors.BlackArrowSmall,
							onHover: null,
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
							onHover: mp => curve.ModSet(CurveMods.AddPoint(mp)),
							onTrigger: startPt => curve.ModSet(CurveMods.AddPoint(startPt))
						),
						Act.Make(
							"Add point (finish)",
							Hotspots.Anywhere,
							Trigger.Up,
							CBase.Cursors.Pen,
							onHover: null,
							onTrigger: curve.ModApply
						)
					)

				)
			)
			.Run(evt).D(d);

		return d;
	}
}


static class Hotspots
{
	public static Func<Pt, Maybe<Pt>> Anywhere => May.Some;
	public static Func<Pt, Maybe<PointId>> CurvePoint(IEntity<CurveModel> curve) => m => curve.V.GetClosestPointTo(m, C.ActivateMoveMouseDistance);
}
