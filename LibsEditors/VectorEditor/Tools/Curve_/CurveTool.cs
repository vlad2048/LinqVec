using System.Reactive.Linq;
using Geom;
using LinqVec;
using LinqVec.Logic;
using VectorEditor.Model;
using LinqVec.Tools;
using LinqVec.Tools.Acts;
using LinqVec.Tools.Events;
using VectorEditor.Tools.Curve_.Mods;
using VectorEditor.Tools.Curve_.Structs;
using LinqVec.Tools.Enums;
using LinqVec.Tools.Events.Utils;
using PowRxVar;

namespace VectorEditor.Tools.Curve_;


sealed class CurveTool(ToolEnv Env, Model<Doc> Doc) : ITool
{
	public Keys Shortcut => Keys.P;

	public (IUndoer, IDisposable) Run(Action reset)
	{
		var d = new Disp();

		Doc.EnableRedrawOnMouseMove(d);

		var evt = Env.GetEvtForTool(this, true, d);

		var curve = Mod.Mem(Curve.Empty(), evt.MousePos).D(d);

		curve.WhenChanged
			.SkipWhile(_ => curve.V.Pts.Length == 0)
			.Where(_ => curve.V.Pts.Length == 0)
			.Take(1)
			.Subscribe(_ =>
			{
				reset();
			}).D(d);


		var gfxState = CurveGfxState.None;

		Action<Option<T>> SetState<T>(CurveGfxState state) => mp =>
		{
			if (mp.IsSome)
				gfxState = state;
		};

		evt.WhenKeyDown(Keys.Escape).Subscribe(_ =>
		{
			reset();
		}).D(d);

		evt.WhenKeyDown(Keys.Enter).Subscribe(_ =>
		{
			Doc.V = Doc.V.AddCurve(curve.V);
			reset();
		}).D(d);


		Act.Loop(
				Act.Amb(

					Act.Seq(
						Act.Make(
							"Move point",
							Hotspots.CurvePoint(curve.V),
							Trigger.Down,
							CBase.Cursors.BlackArrowSmall,
							onHover: SetState<PointId>(CurveGfxState.Edit),
							onTrigger: pointId => curve.Mod = CurveMods.MovePoint(pointId)
						),
						Act.Make(
							"Move point (finish)",
							Hotspots.Anywhere,
							Trigger.Up,
							CBase.Cursors.BlackArrowSmall,
							onHover: null,
							onTrigger: _ => curve.Apply()
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
								curve.Mod = CurveMods.AddPoint(mp);
							},
							onTrigger: startPt =>
							{
								L.WriteLine("Trigger -> Down");
								curve.Mod = CurveMods.AddPoint(startPt);
							}),
						Act.Make(
							"Add point (finish)",
							Hotspots.Anywhere,
							Trigger.Up,
							CBase.Cursors.Pen,
							onHover: SetState<Pt>(CurveGfxState.DragHandle),
							onTrigger: _ =>
							{
								L.WriteLine("Trigger -> Up");
								curve.Apply();
							})
					)

				)
			)
			.Run(evt).D(d);

		Env.WhenPaint.Subscribe(gfx =>
		{
			CurvePainter.Draw(gfx, curve.VModded, gfxState);
		}).D(d);

		return (curve, d);
	}
}


static class Hotspots
{
	public static Func<Pt, Option<Pt>> Anywhere => Option<Pt>.Some;
	public static Func<Pt, Option<PointId>> CurvePoint(Curve curve) => m => curve.GetClosestPointTo(m, C.ActivateMoveMouseDistance);
}
