using System.Reactive.Disposables;
using System.Reactive.Linq;
using LinqVec.Logic;
using LinqVec.Structs;
using LinqVec.Utils;
using PowRxVar;

namespace VectorEditor.Model;



public sealed record DocModel(
	CurveModel[] Curves
)
{
	public static readonly DocModel Empty = new(Array.Empty<CurveModel>());
}


static class Entities
{
	public static readonly Func<ModelMan<DocModel>, Func<DocModel, (DocModel, ISmartId<CurveModel>)>> Curve =
		mm =>
			m =>
			{
				var entity = CurveModel.Empty();
				var id = entity.SmartId(mm);
				var mNext = m.WithCurves(m.Curves.Add(entity));
				return (mNext, id);
			};
}
