using LinqVec.Structs;
using PowBasics.CollectionsExt;
using VectorEditor.Model;

namespace VectorEditor.Panes.ModelTree_;

sealed record ModelNode(
	IId Obj
);

static class TreeMaker
{
	public static TNod<ModelNode>[] ToTree(this DocModel model) =>
		model.Layers.SelectToArray(layer =>
			Nod.Make(new ModelNode(layer),
				layer.Objects.Select(obj => Nod.Make(new ModelNode(obj)))
			)
		);
}