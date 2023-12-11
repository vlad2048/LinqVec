using LinqVec.Logic;
using LinqVec.Tools;

namespace LinqVec;

public sealed record VecEditorInitNfo(
	IUndoer DocUndoer,
	ITool[] Tools
);