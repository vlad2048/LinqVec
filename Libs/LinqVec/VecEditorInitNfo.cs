using LinqVec.Tools;

namespace LinqVec;

public sealed record VecEditorInitNfo(
	IModel Model,
	ITool[] Tools
);