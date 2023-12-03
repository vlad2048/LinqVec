using LinqVec.Logic;
using LinqVec.Utils;

namespace VectorEditor.Model;


public enum ObjType
{
	Curve,
}

public sealed record ObjId(
	ObjType Type,
	Guid Id
);


public sealed record DocModel(
	CurveModel[] Curves
)
{
	public static readonly DocModel Empty = new(Array.Empty<CurveModel>());
}


public static class DocModelOps
{
	public static ObjId CreateCurve(this Undoer<DocModel> model)
	{
		var curve = CurveModel.Empty();
		model.Do(model.V with { Curves = model.V.Curves.Add(curve) });
		return curve.ObjId;
	}
}