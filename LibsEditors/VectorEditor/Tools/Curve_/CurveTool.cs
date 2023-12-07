using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using PowRxVar;
using LinqVec;
using LinqVec.Logic;
using LinqVec.Structs;
using VectorEditor.Model;
using LinqVec.Tools;
using LinqVec.Tools.Events;
using LinqVec.Utils;
using PowMaybe;
using VectorEditor.Tools.Curve_.Mods;
using VectorEditor.Tools.Curve_.Structs;
using LinqVec.Tools.Enums;

namespace VectorEditor.Tools.Curve_;


sealed class CurveTool(ToolEnv env, ModelMan<DocModel> mm) : Tool<DocModel>(env, mm)
{
	public override Keys Shortcut => Keys.F1;

	public override async Task Run(IRoDispBase d)
	{
		var evt = Env.GetEvtForTool(this)
			.ToGrid(Env.Transform)
			.SnapToGrid(Env.Transform)
			.TrackPos(out var mousePos, d)
			.RestrictToGrid()
			.MakeHot(d)
			.ToEvt(e => Env.Curs.Cursor = e);

		//evt.WhenEvt.Log(d);

		var curve = mm.Create(Entities.Curve(mm.V.Layers[0].Id));

		var movePoint_Start = Acts.MovePoint_Start(evt, curve);
		var movePoint_Finish = Acts.MovePoint_Finish(evt, curve, mousePos);
		var addPoint_Start = Acts.AddPoint_Start(evt, curve).Exclude(movePoint_Start);
		var addPoint_Finish = Acts.AddPoint_Finish(evt, curve, mousePos);


		//L.WriteLine("curve.start");
		//Disposable.Create(() => L.WriteLine("curve.dispose")).D(d);


		Act.Loop(

				Act.Amb(

					Act.Seq(
						movePoint_Start.ToSeq(),
						_ => movePoint_Finish.ToSeq()
					),

					Act.Seq(
						addPoint_Start.ToSeq(),
						_ => addPoint_Finish.ToSeq()
					)

				)

			)
			.Run(evt).D(d);
	}
}




static class Acts
{
	public static Act<Pt> AddPoint_Start(Evt evt, IEntity<CurveModel> curve) =>
		new(
			evt,
			Trigger.Down,
			new Hotspot<Pt>(
				May.Some,
				CBase.Cursors.Pen
			),
			OnHover: on => curve.ModSet(CurveMods.AddPoint().If(on)),
			OnTrigger: startPt => curve.ModSet(CurveMods.AddPoint(startPt))
		);

	public static Act<Unit> AddPoint_Finish(Evt evt, IEntity<CurveModel> curve, IRoMayVar<Pt> mousePos) =>
		new(
			evt,
			Trigger.Up,
			new Hotspot<Unit>(
				_ => May.Some(Unit.Default),
				CBase.Cursors.Pen
			),
			OnHover: null,
			OnTrigger: _ => curve.ModApply(mousePos)
		);


	public static Act<PointId> MovePoint_Start(Evt evt, IEntity<CurveModel> curve) =>
		new(
			evt,
			Trigger.Down,
			new Hotspot<PointId>(
				m => curve.V.GetClosestPointTo(m, C.ActivateMoveMouseDistance),
				CBase.Cursors.BlackArrowSmall
			),
			OnHover: null,
			OnTrigger: pointId => curve.ModSet(CurveMods.MovePoint(pointId))
		);

	public static Act<Unit> MovePoint_Finish(Evt evt, IEntity<CurveModel> curve, IRoMayVar<Pt> mousePos) =>
		new(
			evt,
			Trigger.Up,
			new Hotspot<Unit>(
				_ => May.Some(Unit.Default),
				CBase.Cursors.BlackArrowSmall
			),
			OnHover: null,
			OnTrigger: _ => curve.ModApply(mousePos)
		);
}
