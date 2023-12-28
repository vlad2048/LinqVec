using BrightIdeasSoftware;
using LinqVec.Logic;

namespace LinqVec;

[Flags]
public enum EditorLogicCaps
{
	SupportLayoutPane = 1,
}

public abstract class EditorLogic<Doc>
{
	public abstract EditorLogicCaps Caps { get; }
	public abstract IUndoerReadOnly<Doc> Init(VecEditor vecEditor, Doc? initModel, Disp d);

	public virtual void SetupLayoutPane(TreeListView tree, IObservable<Option<Doc>> doc, Disp d) => throw new NotImplementedException();
}