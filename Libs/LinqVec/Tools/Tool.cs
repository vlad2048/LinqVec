namespace LinqVec.Tools;

public sealed record ToolActions(
	Action Reset
);

public interface ITool<TDoc, TState>
{
	string Name { get; }
	Bitmap? Icon { get; }
	Keys Shortcut { get; }
	Disp Run(ToolEnv<TDoc, TState> Env, ToolActions toolActions);
}


public sealed class EmptyTool<TDoc, TState> : ITool<TDoc, TState>
{
	public string Name => "_";
	public Bitmap? Icon => null;
	public Keys Shortcut => 0;
	public Disp Run(ToolEnv<TDoc, TState> Env, ToolActions toolActions) => MkD();

	private EmptyTool()
	{
	}

	public static readonly ITool<TDoc, TState> Instance = new EmptyTool<TDoc, TState>();
}