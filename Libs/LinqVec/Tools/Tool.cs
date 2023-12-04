using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using LinqVec.Logic;
using PowRxVar;

namespace LinqVec.Tools;

public interface ITool
{
	string Name { get; }
	Keys Shortcut { get; }
	IDisposable RunRest(Action<Pt> startFun);
	IDisposable Run(Pt startPt);
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

	public abstract IDisposable RunRest(Action<Pt> startFun);
	public abstract IDisposable Run(Pt startPt);
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

	public IDisposable RunRest(Action<Pt> startFun)
	{
		setDefaultCursor();
		return Disposable.Empty;
	}

	public IDisposable Run(Pt startPt) => throw new ArgumentException();
}