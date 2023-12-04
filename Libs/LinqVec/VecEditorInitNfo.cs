using System.Reactive;
using LinqVec.Tools;

namespace LinqVec;

public sealed record VecEditorInitNfo(
	IObservable<Unit> WhenToolResetRequired,
	ITool[] Tools
);