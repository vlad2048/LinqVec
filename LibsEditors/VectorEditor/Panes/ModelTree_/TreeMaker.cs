using LinqVec.Structs;
using PowBasics.CollectionsExt;
using VectorEditor.Model;

namespace VectorEditor.Panes.ModelTree_;

sealed record DocNode(
	IId Obj
);

static class TreeMaker
{
	public static TNod<DocNode>[] ToTree(this Doc doc) =>
		doc.Layers.SelectToArray(layer =>
			Nod.Make(new DocNode(layer),
				layer.Objects.Select(obj => Nod.Make(new DocNode(obj)))
			)
		);
}
