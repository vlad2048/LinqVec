namespace LinqVec.Tools;

public sealed record ToolActions(
	Action Reset
);

public interface ITool<TDoc>
{
	string Name { get; }
	Bitmap? Icon { get; }
	Keys Shortcut { get; }
	Disp Run(ToolEnv<TDoc> Env, ToolActions toolActions);
}


public sealed class EmptyTool<TDoc> : ITool<TDoc>
{
	public string Name => "_";
	public Bitmap? Icon => null;
	public Keys Shortcut => 0;
	public Disp Run(ToolEnv<TDoc> Env, ToolActions toolActions) => MkD();

	private EmptyTool()
	{
	}

	public static readonly ITool<TDoc> Instance = new EmptyTool<TDoc>();
}