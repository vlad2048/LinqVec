using LinqVec.Logic;

namespace LinqVec.Tools;

public sealed record ToolActions(
	Action Reset
);

public interface ITool
{
	Keys Shortcut { get; }
	Disp Run(ToolActions toolActions);
}
