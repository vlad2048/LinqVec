using LinqVec.Logic;
using LinqVec.Tools;

namespace LinqVec;

public sealed record VecEditorInitNfo<TDoc>(
	Unmod<TDoc> Doc,
	ITool<TDoc>[] Tools
);