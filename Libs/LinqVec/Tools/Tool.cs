using LinqVec.Logic;

namespace LinqVec.Tools;

public interface ITool
{
	Keys Shortcut { get; }
	(IUndoer, IDisposable) Run(Action reset);
}
