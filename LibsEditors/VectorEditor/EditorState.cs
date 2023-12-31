using PowBasics.CollectionsExt;
using ReactiveVars;

namespace VectorEditor;

public sealed record EditorState(
	Arr<Guid> Selection
)
{
	public override string ToString() => $"Selection:{Selection.JoinText()}";

	public static readonly EditorState Empty = new([]);
}



static class EditorStateExt
{
	public static void Select(this IRwVar<EditorState> state, Guid[] ids) => state.V = state.V with { Selection = ids };
	public static void SelectF(this IRwVar<EditorState> state, Func<Arr<Guid>, Arr<Guid>> f) => state.V = state.V with { Selection = f(state.V.Selection) };
}