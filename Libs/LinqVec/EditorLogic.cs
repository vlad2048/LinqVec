using System.Reactive.Linq;
using BrightIdeasSoftware;
using LinqVec.Structs;
using LinqVec.Tools;
using PtrLib;

namespace LinqVec;

[Flags]
public enum EditorLogicCaps
{
	SupportLayoutPane = 1,
}


public abstract class EditorLogicMaker
{
	public abstract EditorLogicCaps Caps { get; }
	public abstract EditorLogic Make(Option<string> filename, ToolEnv env, Disp d);
}


public abstract class EditorLogic
{
	public abstract IDocHolder DocHolder { get; }
	public abstract ITool[] Tools { get; }

	public abstract void Save(string filename);

	public virtual void SetupLayoutPane(
		TreeListView tree,
		Disp d
	) => throw new NotImplementedException();
}