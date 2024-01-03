namespace GeomInt;

public readonly record struct PtInt(int X, int Y)
{
	public static readonly PtInt Empty = new(0, 0);
	public override string ToString() => $"{X},{Y}";
	public bool Equals(PtInt other) => X == other.X && Y == other.Y;
	public override int GetHashCode() => HashCode.Combine(X, Y);
	public static PtInt operator +(PtInt a, PtInt b) => new(a.X + b.X, a.Y + b.Y);
	public static PtInt operator -(PtInt a, PtInt b) => new(a.X - b.X, a.Y - b.Y);
	public static PtInt operator -(PtInt a) => new(-a.X, -a.Y);
	public double Length => Math.Sqrt(X * X + Y * Y);
}