namespace GeomInt;

public readonly record struct RInt
{
	public int X { get; }
	public int Y { get; }
	public int Width { get; }
	public int Height { get; }

	public int Right => X + Width;
	public int Bottom => Y + Height;

	public PtInt Pos => new(X, Y);
	public PtInt Size => new(Width, Height);
	public static readonly RInt Empty = new(0, 0, 0, 0);
	public bool IsDegenerate => Size.X == 0 || Size.Y == 0;

	public RInt(int x, int y, int width, int height)
	{
		if (width < 0 || height < 0)
			throw new ArgumentException();
		X = x;
		Y = y;
		Width = width;
		Height = height;
	}

	public RInt(PtInt pos, PtInt size) : this(pos.X, pos.Y, size.X, size.Y)
	{
	}

	public RInt(PtInt size) : this(0, 0, size.X, size.Y)
	{
	}

	public static RInt MakeOrEmpty(int x, int y, int width, int height) => (width > 0 && height > 0) switch
	{
		true => new RInt(x, y, width, height),
		false => Empty
	};

	public override string ToString() => $"{X},{Y} {Size}";

	public static RInt operator +(RInt a, PtInt b) => a == Empty ? Empty : new RInt(a.Pos + b, a.Size);
	public static RInt operator +(PtInt b, RInt a) => a == Empty ? Empty : new RInt(a.Pos + b, a.Size);
	public static RInt operator -(RInt a, PtInt b) => a == Empty ? Empty : new RInt(a.Pos - b, a.Size);
	public static RInt operator -(PtInt b, RInt a) => a == Empty ? Empty : new RInt(a.Pos - b, a.Size);

	public PtInt Center => new(X + Width / 2, Y + Height / 2);
	public static RInt FromCenterAndSize(PtInt center, PtInt size) => new(center.X - size.X / 2, center.Y - size.Y / 2, size.X, size.Y);

	public RInt Intersection(RInt a)
	{
		var x = Math.Max(a.X, X);
		var num1 = Math.Min(a.X + a.X, X + Width);
		var y = Math.Max(a.Y, Y);
		var num2 = Math.Min(a.Y + a.Y, Y + Height);
		/*
            In WinDX (for Pop nodes), it's important for to have the intersection of a rectangle with
            zero size to be a rectangle with zero size at the correct location.

            This is why we have:
            num1 >= x && num2 >= y
            and not:
            num1 > x && num2 > y
        */
		return num1 >= x && num2 >= y ? new RInt(x, y, num1 - x, num2 - y) : RInt.Empty;
	}
}