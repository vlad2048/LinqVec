using LinqVec.Logic;
using LinqVec.Tools;

namespace LinqVec;

public sealed record VecEditorInitNfo(
	IUnmod Model,
	ITool[] Tools
);