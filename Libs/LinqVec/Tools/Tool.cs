using LinqVec.Logic;
using PowRxVar;

namespace LinqVec.Tools;


public interface ITool
{
	string Name { get; }
	Keys Shortcut { get; }
	Task Run(IRoDispBase d);
}

public abstract class Tool<M> : ITool
{
	protected ToolEnv Env { get; }
	protected ModelMan<M> MM { get; }

	public string Name => GetType().Name[..^4];
	public abstract Keys Shortcut { get; }

	protected Tool(ToolEnv env, ModelMan<M> mm)
	{
		Env = env;
		MM = mm;
	}

	public abstract Task Run(IRoDispBase d);
}


sealed class NoneTool : ITool
{
	private readonly Action setDefaultCursor;

	public NoneTool(Action setDefaultCursor)
	{
		this.setDefaultCursor = setDefaultCursor;
	}

	public string Name => GetType().Name[..^4];

	public Keys Shortcut => Keys.Escape;

	public Task Run(IRoDispBase d)
	{
		setDefaultCursor();
		return Task.CompletedTask;
	}
}
