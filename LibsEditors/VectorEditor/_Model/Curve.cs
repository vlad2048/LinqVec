﻿using System.Reactive.Linq;
using Geom;
using LinqVec.Interfaces;
using LinqVec.Utils;
using PowBasics.CollectionsExt;
using PtrLib;
using ReactiveVars;
using VectorEditor._Model.Interfaces;
using VectorEditor._Model.Structs;
using VectorEditor._Model.Structs.Enums;

namespace VectorEditor._Model;

public sealed record Curve(
	Guid Id,
	CurvePt[] Pts,
	bool Closed
) : IObj
{
	public static Curve Empty() => new(Guid.NewGuid(), [], false);

	public R BoundingBox => GetDrawPoints(this).GetBBox();
	public double DistanceToPoint(Pt pt) => GetDrawPoints(this).DistanceToPoint(pt);

	public override string ToString() => $"Curve({Pts.Select(e => $"({e})").JoinText(",")})";


	private static Pt[] GetDrawPoints(Curve model) =>
		model.Pts
			.SelectMany(p => new[]
			{
				p.HLeft,
				p.P,
				p.HRight
			})
			.Skip(1)
			.SkipLast(1)
			.ToArray();
}


// ********
// * Funs *
// ********
static class CurveFuns
{
	public static Doc Create_SetFun(Doc doc, Curve curve) =>
		doc with
		{
			Layers = [
				SetCurve(doc.Layers[0], curve),
				.. doc.Layers.Skip(1)
			]
		};

	public static bool Create_ValidFun(Curve curve) => curve.Pts.Length > 1;

	public static Doc Edit_RemoveFun(Doc doc, Curve curve) => doc.DeleteObjects([curve.Id]);

	private static Layer SetCurve(Layer layer, Curve curve) => layer with { Objects = layer.Objects.AddSet(curve) };

	private static T[] AddSet<T>(this T[] xs, T x) where T : IId
	{
		var idx = xs.IndexOf(e => e.Id == x.Id);
		var list = xs.ToList();
		if (idx == -1) return list.Append(x).ToArray();
		list[idx] = x;
		return list.ToArray();
	}
}


// ********
// * Mods *
// ********
static class CurveMods
{
	public static Curve MovePoint(this Curve curve, PointId pointId, Pt ptEnd) =>
		curve with
		{
			Pts = curve.Pts.SetIdxArr(pointId.Idx, e => e.Move(pointId.Type, ptEnd))
		};

	public static Curve AddPoint(this Curve curve, Pt ptStart, Pt ptEnd) =>
		curve with
		{
			Pts = curve.Pts.AddArr(CurvePt.Make(ptStart, ptEnd))
		};

	public static bool CanClose(this Curve curve) => curve is { Closed: false, Pts.Length: >= 2 };
	public static Curve CloseCurve(this Curve curve, Pt mouse)
	{
		if (!curve.CanClose()) throw new ArgumentException();
		return curve with {
			Pts = curve.Pts.SetIdxArr(0, p => p with { HRight = mouse, HLeft = p.P + p.P - mouse }),
			Closed = true,
		};
	}


	private static CurvePt Move(this CurvePt pt, PointType type, Pt pos) => type switch
	{
		PointType.Point => pt.MovePoint(pos),
		PointType.LeftHandle => pt.MoveLeftHandle(pos),
		PointType.RightHandle => pt.MoveRightHandle(pos),
		_ => throw new ArgumentException()
	};

	private static CurvePt MovePoint(this CurvePt p, Pt pos)
	{
		var delta = pos - p.P;
		return new CurvePt(pos, p.HLeft + delta, p.HRight + delta);
	}

	private static CurvePt MoveLeftHandle(this CurvePt p, Pt pos)
	{
		var delta = pos - p.P;
		return new CurvePt(p.P, pos, p.P - delta);
	}
	private static CurvePt MoveRightHandle(this CurvePt p, Pt pos)
	{
		var delta = pos - p.P;
		return new CurvePt(p.P, p.P - delta, pos);
	}
}


// *********
// * Utils *
// *********
static class CurveUtils
{
	private sealed record PtNfo(PointId Id, double Distance);

	
	public static Option<PointId> GetClosestPointTo(this Curve curve, Pt pt, double threshold)
	{
		PtNfo Mk(CurvePt mp, int idx, PointType type) => new(new PointId(idx, type), (mp.GetPt(type) - pt).Length);

		Option<PointId> For(PointType type) =>
			curve.Pts
				.Select((e, i) => Mk(e, i, type))
				.OrderByDescending(e => e.Distance)
				.Where(e => e.Distance < threshold)
				.Select(e => e.Id)
				.FirstOrOption();

		return OptionExt.AggregateArr(
			For(PointType.Point),
			For(PointType.LeftHandle),
			For(PointType.RightHandle)
		);
	}

	public static Option<StartOrEnd> GetExtremityAt(this Curve curve, Pt p, double threshold) =>
		from pointId in curve.GetClosestPointTo(p, threshold)
		where pointId.Idx == 0 || pointId.Idx == curve.Pts.Length - 1
		select pointId.Idx switch {
			0 => StartOrEnd.Start,
			_ => StartOrEnd.End
		};
}