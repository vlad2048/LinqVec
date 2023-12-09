using LinqVec.Logic;
using LinqVec.Structs;
using LinqVec.Utils;
using PowMaybe;

namespace VectorEditor.Model;

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
}