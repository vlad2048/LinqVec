using System.Reactive.Disposables;
using LinqVec.Tools._Base;

namespace LinqVec.Tools.None_;

public class NoneTool : Tool
{
    public override string Name => "none";
    public override Keys Shortcut => Keys.Escape;

    public override (Tool, IDisposable) Init(IToolEnv env) => (this, Disposable.Empty);
}