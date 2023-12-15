﻿using System.Reactive.Linq;
using Geom;
using LinqVec;
using LinqVec.Logic;
using VectorEditor.Model;
using LinqVec.Tools;
using LinqVec.Tools.Acts;
using LinqVec.Tools.Acts.Enums;
using LinqVec.Tools.Events;
using VectorEditor.Tools.Curve_.Mods;
using VectorEditor.Tools.Curve_.Structs;
using LinqVec.Tools.Events.Utils;
using LinqVec.Utils;
using PowRxVar;

namespace VectorEditor.Tools.Curve_;


sealed class CurveTool(ToolEnv Env, Model<Doc> Doc) : ITool
{
	public Keys Shortcut => Keys.P;

	public IDisposable Run(ToolActions toolActions)
	{
		var d = new Disp();

		Doc.EnableRedrawOnMouseMove(d);

		var evt = Env.GetEvtForTool(this, true, d);


		var curve = new MemModder<Curve>(Curve.Empty()).D(d);
		toolActions.SetUndoer(curve);
		var gfxState = CurveGfxState.None;

		evt.WhenKeyDown(Keys.Escape).Subscribe(_ => toolActions.Reset()).D(d);
		evt.WhenKeyDown(Keys.Enter).Subscribe(_ =>
		{
			if (curve.Get().Pts.Length > 0)
				Doc.V = Doc.V.AddObject(curve.Get());
			toolActions.Reset();
		}).D(d);


		ActRunner.Run(
			evt,
			toolActions.WhenUndoRedo,

			[
				Act.Make(
					"Move point",
					Hotspots.CurvePoint(curve),
					Gesture.Drag,
					CBase.Cursors.BlackArrowSmall,
					Actions.Drag<Curve, PointId>(curve, CurveMods.MovePoint, () => gfxState = CurveGfxState.Edit, () => gfxState = CurveGfxState.Edit)
				),
				Act.Make(
					"Add point",
					Hotspots.Anywhere,
					Gesture.Drag,
					CBase.Cursors.Pen,
					Actions.Drag<Curve, Pt>(curve, new Mod<Curve>(CurveMods.AddPoint), () => gfxState = CurveGfxState.AddPoint, () => gfxState = CurveGfxState.DragHandle)
				),
			]

		).D(d);


		Env.WhenPaint.Subscribe(gfx =>
		{
			CurvePainter.Draw(gfx, curve.ModGet(evt.MousePos.V), gfxState);
		}).D(d);

		return d;
	}
}


static class Hotspots
{
	public static Func<Pt, Option<Pt>> Anywhere => Option<Pt>.Some;
	public static Func<Pt, Option<PointId>> CurvePoint(IModder<Curve> curve) => m => curve.Get().GetClosestPointTo(m, C.ActivateMoveMouseDistance);
}



/*
// State:	Pts[0], ..., Pts[n-1]

// - draw Marker @ MousePos
// - draw RedLine @ (Pts[n-1].P, Pts[n-1].HRight) -> (MousePos, MousePos)
// - draw Handles @ Pts[n-1]
// - finish: MouseDown(DownPos <- MousePos)
public sealed record AddPointPre_CurveModelEditState : ICurveModelEditState;

// - draw Marker @ DownPos
// - draw RedLine @ (Pts[n-1].P, Pts[n-1].HRight) -> (DownPos-(MousePos-DownPos), DownPos)
// - draw Handles @ (DownPos-(MousePos-DownPos), DownPos, DownPos+(MousePos-DownPos))
// - finish: MouseUp => Model.AddPoint(DownPos-(MousePos-DownPos), DownPos, DownPos+(MousePos-DownPos))
public sealed record AddPointHandleDrag_CurveModelEditState(Pt DownPos) : ICurveModelEditState;

// State:	Pts[0], ..., Pts[n-1], Pts[n]
*/

