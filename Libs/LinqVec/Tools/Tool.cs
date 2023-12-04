namespace LinqVec.Tools;


public abstract class Tool
{
    public abstract string Name { get; }
    public abstract Keys Shortcut { get; }

    public abstract (Tool, IDisposable) Init(ToolEnv env);
}
