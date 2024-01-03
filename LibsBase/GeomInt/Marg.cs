namespace GeomInt;

public readonly record struct Marg
{
	public int Top { get; }
	public int Right { get; }
	public int Bottom { get; }
	public int Left { get; }

	public Marg(int top, int right, int bottom, int left)
	{
		if (top < 0 || right < 0 || bottom < 0 || left < 0) throw new ArgumentException();
		Top = top;
		Right = right;
		Bottom = bottom;
		Left = left;
	}

	public Marg MgUp(int top) => new(top, Right, Bottom, Left);
	public Marg MgRight(int right) => new(Top, right, Bottom, Left);
	public Marg MgDown(int bottom) => new(Top, Right, bottom, Left);
	public Marg MgLeft(int left) => new(Top, Right, Bottom, left);

	public string FmtConcise() => (this == default) switch
	{
		true => string.Empty,
		false when (Top == Right && Top == Bottom && Top == Left) => $"mg({Top})",
		_ => $"mg({Top},{Right},{Bottom},{Left})"
	};
}

public static class MargUtils
{
	public static int Dir(this Marg m, Dir dir) => dir switch
	{
		GeomInt.Dir.Horz => m.Left + m.Right,
		GeomInt.Dir.Vert => m.Top + m.Bottom,
		_ => throw new ArgumentException()
	};

	public static Marg Mirror(this Marg m) => new(m.Left, m.Bottom, m.Right, m.Top);
}
