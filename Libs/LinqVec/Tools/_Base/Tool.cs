namespace LinqVec.Tools._Base;


public abstract class Tool
{
    public abstract string Name { get; }
    public abstract Keys Shortcut { get; }

    public abstract (Tool, IDisposable) Init(IToolEnv env);
}
