using PowMaybe;

namespace LinqVec.Tools.Curve_.Model;


interface ICurveMod;
sealed record NoneCurveMod : ICurveMod;
sealed record AddPointCurveMod(Pt? StartPos) : ICurveMod;
sealed record MovePointCurveMod(PointId Id) : ICurveMod;
sealed record RemovePointCurveMod(int Idx) : ICurveMod;

sealed record CurveModel(
	CurvePt[] Pts
)
{
	public static readonly CurveModel Empty = new(Array.Empty<CurvePt>());
}





static class CurveModelOps
{
	private sealed record PtNfo(CurvePt P, PointId Id, double Distance);

	public static Pt GetPointById(this CurveModel model, PointId id) => model.Pts[id.Idx].GetPt(id.Type);

	public static Maybe<PointId> GetClosestPointTo(this CurveModel model, Pt pt, double threshold)
	{
		//if (mayPt.IsNone(out var pt)) return May.None<PointId>();
		PtNfo Mk(CurvePt mp, int idx, PointType type) => new (mp, new PointId(idx, type), (mp.GetPt(type) - pt).Length);

		Maybe<PointId> For(PointType type) =>
			model.Pts
				.Select((e, i) => Mk(e, i, type))
				.OrderByDescending(e => e.Distance)
				.Where(e => e.Distance < threshold)
				.Select(e => e.Id)
				.FirstOrMaybe();

		return Aggregate(
			For(PointType.Point),
			For(PointType.LeftHandle),
			For(PointType.RightHandle)
		);
	}


	public static CurveModel ApplyMod(this CurveModel model, ICurveMod mod, Maybe<Pt> pos) => pos.IsSome(out var p) switch
	{
		true => model.ApplyMod(mod, p),
		false => model
	};

	public static CurveModel ApplyMod(this CurveModel model, ICurveMod mod, Pt pos) => mod switch
	{
		NoneCurveMod
			=> model,
		AddPointCurveMod { StartPos: var startPos }
			=> model with { Pts = model.Pts.Add(CurvePt.Make(startPos, pos)) },
		MovePointCurveMod { Id: var id }
			=> model with { Pts = model.Pts.ChangeIdx(id.Idx, e => e.Move(id.Type, pos)) },
		RemovePointCurveMod { Idx: var idx }
			=> model with { Pts = model.Pts.RemoveIdx(idx) },
		_ => throw new ArgumentException()
	};

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

	private static T[] Add<T>(this T[] arr, T e) => arr.ToList().Append(e).ToArray();
	private static T[] ChangeIdx<T>(this T[] arr, int idx, Func<T, T> fun)
	{
		var list = arr.Take(idx).ToList();
        list.Add(fun(arr[idx]));
        list.AddRange(arr.Skip(idx + 1));
        return list.ToArray();
	}
	private static T[] RemoveIdx<T>(this T[] arr, int idx)
	{
		var list = arr.Take(idx).ToList();
		list.AddRange(arr.Skip(idx + 1));
		return list.ToArray();
	}

	private static Maybe<T> Aggregate<T>(params Maybe<T>[] arr)
	{
		foreach (var elt in arr)
			if (elt.IsSome())
				return elt;
		return May.None<T>();
	}
}
