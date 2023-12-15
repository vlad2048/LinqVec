using System.Reactive.Linq;
using LinqVec.Structs;
using LinqVec.Utils;
using LinqVec.Logic;

namespace VectorEditor.Model;


static class Entities
{
	/*
	public static Lens<IVisualObjSer> Visual(Model<Doc> doc, IVisualObjSer obj) => obj switch
	{
		Curve e => Curve(doc, e).Cast<Curve, IVisualObjSer>(),
		_ => throw new ArgumentException()
	};

	private static Lens<U> Cast<T, U>(this Lens<T> lens) where T : U where U : IId => new(
		() => lens.Get(),
		objNext => lens.Set((T)objNext),
		lens.WhenDisappear
	);


	public static Lens<Curve> Curve(Model<Doc> doc, Curve obj) => new(
		() => doc.V.Layers.SelectMany(e => e.Objects).OfType<Curve>().Single(e => e.Id == obj.Id),
		objNext =>
		{
			if (objNext.Id != obj.Id) throw new ArgumentException();
			var layer = doc.V.Layers.Single(e => e.Objects.Any(f => f.Id == obj.Id));
			doc.V = doc.V.WithLayers(doc.V.Layers.ChangeId(layer.Id, layer_ => layer_.WithObjects(layer_.Objects.SetId(objNext))));
		},
		doc.WhenChanged.Where(_ => doc.V.Layers.SelectMany(e => e.Objects).OfType<Curve>().All(e => e.Id != obj.Id))
	);

	private static Doc WithLayers(this Doc m, Layer[] xs) => m with { Layers = xs };
	private static Layer WithObjects(this Layer m, IVisualObjSer[] xs) => m with { Objects = xs };
	*/


	/*
	public static Curve CreateAndAddCurve(this LinqVec.Logic.Model<Doc> doc, Guid layerId)
	{
		var curve = Model.Curve.Empty();
		doc.V = doc.V.WithLayers(doc.V.Layers.ChangeId(layerId, layer => layer.WithObjects(layer.Objects.AddId(curve))));
		return curve;
	}


	public static readonly EntityNfo<Doc, Curve> Curve = new(
		Add: (doc, obj) => doc.WithLayers(doc.Layers.ChangeId(doc.Layers[0].Id, layer => layer.WithObjects(layer.Objects.AddId(obj)))),
		Del: (doc, id) => doc.WithLayers(doc.Layers.ChangeId(GetLayerId(doc, id), layer => layer.WithObjects(layer.Objects.RemoveId(id)))),
		Get: (doc, id) => (Curve)doc.Layers.GetId(GetLayerId(doc, id)).Objects.GetId(id),
		Set: (doc, id, obj) => doc.WithLayers(doc.Layers.ChangeId(GetLayerId(doc, id), layer => layer.WithObjects(layer.Objects.SetId(obj))))
	);

	private static Guid GetLayerId(Doc doc, Guid objId) => doc.Layers.Single(layer => layer.Objects.Any(e => e.Id == objId)).Id;
	*/

	/*{
		Guid GetLayerId(Doc doc) => doc.Layers.Single(layer => layer.Objects.Any(e => e.Id == objId)).Id;

		return new EntityNfo<Doc, Curve>(
			Get: doc =>
			{
				var layerId = GetLayerId(doc);
				return (Curve)doc.Layers.GetId(layerId).Objects.GetId(objId);
			},
			Set: (doc, objNext) =>
			{
				var layerId = GetLayerId(doc);
				return doc.WithLayers(doc.Layers.ChangeId(layerId, layer => layer.WithObjects(layer.Objects.SetId(objNext))));
			},
			Del: doc =>
			{
				var layerId = GetLayerId(doc);
				return doc.WithLayers(doc.Layers.ChangeId(layerId, layer => layer.WithObjects(layer.Objects.RemoveId(objId))));
			}
		);
	}*/


	/*
	public static TrkCreate<Doc, Curve> CurveCreate(Layer layer)
	{
		var obj = Curve.Empty();
		var objId = obj.Id;
		var layerId = layer.Id;
		return new TrkCreate<Doc, Curve>(
			Create: doc =>
			{
				var docNext = doc.WithLayers(doc.Layers.ChangeId(layerId, layer => layer.WithObjects(layer.Objects.Add(obj))));
				return (docNext, obj);
			},
			Get: doc => (Curve)doc.Layers.GetId(layerId).Objects.GetId(objId),
			Set: (doc, objNext) => doc.WithLayers(doc.Layers.ChangeId(layerId, layer => layer.WithObjects(layer.Objects.SetId(objNext)))),
			Delete: doc => doc.WithLayers(doc.Layers.ChangeId(layerId, layer => layer.WithObjects(layer.Objects.RemoveId(objId))))
		);
	}

	public static TrkGet<Doc, Curve> CurveGet(Curve obj)
	{
		var objId = obj.Id;
		Guid GetLayerId(Doc doc) => doc.Layers.Single(layer => layer.Objects.Any(e => e.Id == obj.Id)).Id;

		return new TrkGet<Doc, Curve>(
			Get: doc =>
			{
				var layerId = GetLayerId(doc);
				return (Curve)doc.Layers.GetId(layerId).Objects.GetId(objId);
			},
			Set: (doc, objNext) =>
			{
				var layerId = GetLayerId(doc);
				return doc.WithLayers(doc.Layers.ChangeId(layerId, layer => layer.WithObjects(layer.Objects.SetId(objNext))));
			},
			Delete: doc =>
			{
				var layerId = GetLayerId(doc);
				return doc.WithLayers(doc.Layers.ChangeId(layerId, layer => layer.WithObjects(layer.Objects.RemoveId(objId))));
			});
	}
	*/


	//private static Doc WithLayers(this Doc m, Layer[] xs) => m with { Layers = xs };
	//private static Layer WithObjects(this Layer m, IVisualObjSer[] xs) => m with { Objects = xs };
}



/*
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
*/