using LinqVec.Logic;

namespace LinqVec.Tools;

public sealed record ToolActions(
	Action Reset,
	Action<IUndoer> SetUndoer,
	IObservable<Unit> WhenUndoRedo
);

public interface ITool
{
	Keys Shortcut { get; }
	IDisposable Run(ToolActions toolActions);
}
