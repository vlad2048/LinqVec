namespace LinqVec.Tools;

public sealed record ToolNfo(
	string Name,
	Bitmap? Icon,
	Keys Shortcut
)
{
	public static readonly ToolNfo Empty = new("_", null, 0);
}

public interface ITool
{
	ToolNfo Nfo { get; }
	void Run(Disp d);
}



public sealed class EmptyTool : ITool
{
	public ToolNfo Nfo => ToolNfo.Empty;
	public void Run(Disp d) {}

	private EmptyTool()
	{
	}

	public static readonly ITool Instance = new EmptyTool();
}