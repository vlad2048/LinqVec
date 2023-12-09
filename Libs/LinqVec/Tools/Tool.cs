using System.Reactive.Disposables;
using LinqVec.Logic;
using PowRxVar;

namespace LinqVec.Tools;


public interface ITool
{
	string Name { get; }
	Keys Shortcut { get; }
	IDisposable Run(Action reset);
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

	public abstract IDisposable Run(Action reset);
}
