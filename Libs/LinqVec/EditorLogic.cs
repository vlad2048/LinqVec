using BrightIdeasSoftware;
using LinqVec.Tools;

namespace LinqVec;

[Flags]
public enum EditorLogicCaps
{
	SupportLayoutPane = 1,
}

public abstract class EditorLogic<TDoc, TState>
{
	public abstract EditorLogicCaps Caps { get; }

	public abstract ITool<TDoc, TState>[] Tools { get; }

	public abstract TDoc LoadOrCreate(Option<string> file);
	public abstract void Save(string file, TDoc doc);

	public abstract void Init(
		ToolEnv<TDoc, TState> env,
		Disp d
	);

	public virtual void SetupLayoutPane(
		TreeListView tree,
		IObservable<Option<TDoc>> doc,
		Disp d
	) => throw new NotImplementedException();
}