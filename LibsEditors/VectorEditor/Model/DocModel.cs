using System.Reactive.Disposables;
using System.Reactive.Linq;
using LinqVec.Logic;
using LinqVec.Structs;
using LinqVec.Tools;
using LinqVec.Utils;
using PowMaybe;
using PowRxVar;
using VectorEditor.Model.Structs;

namespace VectorEditor.Model;


public interface ILayerObject : IId;

public sealed record LayerModel(
	Guid Id,
	ILayerObject[] Objects
) : IId
{
	public static LayerModel Empty() => new(
		Guid.NewGuid(),
		Array.Empty<ILayerObject>()
	);
}


public sealed record DocModel(
	LayerModel[] Layers
)
{
	public static readonly DocModel Empty = new(new[] { LayerModel.Empty() });
}



static class Entities
{
	public static readonly Func<ModelMan<DocModel>, IEntity<DocModel, LayerModel>> Layer =
		mm =>
		{
			var model = LayerModel.Empty();
			return new Entity<DocModel, LayerModel>(
				mm,
				init: model,
				isValid: (m, isCommited) => isCommited switch
				{
					false => !m.Layers.ContainsId(model.Id),
					true => m.Layers.ContainsId(model.Id),
				},
				add: (m, e) => m.WithLayers(m.Layers.AddId(e)),
				delete: m => m.WithLayers(m.Layers.RemoveId(model.Id)),
				get: m => m.Layers.GetId(model.Id),
				set: (m, e) => m.WithLayers(m.Layers.SetId(e))
			);
		};

	public static Func<ModelMan<DocModel>, IEntity<DocModel, CurveModel>> Curve(Guid layerId) =>
		mm =>
		{
			var model = CurveModel.Empty();
			return new Entity<DocModel, CurveModel>(
				mm,
				init: model,
				isValid: (m, isCommited) =>
				{
					if (m.Layers.GetMayId(layerId).IsNone(out var layer)) return false;
					return isCommited switch
					{
						false => !layer.Objects.ContainsId(model.Id),
						true => layer.Objects.ContainsIdAndIsOfType<ILayerObject, CurveModel>(model.Id)
					};
				},
				add: (m, e) =>
				{
					var layer = m.Layers.GetId(layerId);
					var layerNext = layer.WithObjects(layer.Objects.AddId(e));
					return m.WithLayers(m.Layers.SetId(layerNext));
				},
				delete: m =>
				{
					var layer = m.Layers.GetId(layerId);
					var layerNext = layer.WithObjects(layer.Objects.RemoveId(model.Id));
					return m.WithLayers(m.Layers.SetId(layerNext));
				},
				get: m =>
				{
					var layer = m.Layers.GetId(layerId);
					return (CurveModel)layer.Objects.GetId(model.Id);
				},
				set: (m, e) =>
				{
					var layer = m.Layers.GetId(layerId);
					var layerNext = layer.WithObjects(layer.Objects.SetId(e));
					return m.WithLayers(m.Layers.SetId(layerNext));
				}
			);
		};



	private static DocModel WithLayers(this DocModel m, LayerModel[] xs) => m with { Layers = xs };
	private static LayerModel WithObjects(this LayerModel m, ILayerObject[] xs) => m with { Objects = xs };


	/*public static readonly Func<ModelMan<DocModel>, Func<DocModel, (DocModel, ISmartId<CurveModel>)>> Curve =
		mm =>
			m =>
			{
				var entity = CurveModel.Empty();
				var id = entity.SmartId(mm);
				var mNext = m.WithCurves(m.Curves.Add(entity));
				return (mNext, id);
			};*/
}
