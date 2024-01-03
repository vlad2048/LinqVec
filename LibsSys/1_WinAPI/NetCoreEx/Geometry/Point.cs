using System.Globalization;
using System.Runtime.InteropServices;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace WinAPI.NetCoreEx.Geometry;

[StructLayout(LayoutKind.Sequential)]
public struct Point : IEquatable<Point>
{
    public int  X, Y;

    public Point(int  x, int  y)
    {
		X = x;
		Y = y;
    }

    public bool Equals(Point other) => X == other.X && Y == other.Y;
    public override bool Equals(object? obj) => obj is Point point && Equals(point);

    public override int GetHashCode()
    {
        unchecked { return (X*397) ^ Y; }
    }

    public static bool operator ==(Point left, Point right) => left.Equals(right);
    public static bool operator !=(Point left, Point right) => !(left == right);

    public void Offset(int  x, int  y) {
		X += x;
		Y += y;
	}

	public void Set(int  x, int  y) {
		X = x;
		Y = y;
	}

	public override string ToString() {
		var culture = CultureInfo.CurrentCulture;
        return $"{{ X = {X.ToString(culture)}, Y = {Y.ToString(culture)} }}";
    }

	public bool IsEmpty => X == 0 && Y == 0;
}

[StructLayout(LayoutKind.Sequential)]
public struct PointS : IEquatable<PointS>
{
    public short  X, Y;

    public PointS(short  x, short  y)
    {
		X = x;
		Y = y;
    }

    public bool Equals(PointS other) => X == other.X && Y == other.Y;
    public override bool Equals(object? obj) => obj is PointS s && Equals(s);

    public override int GetHashCode()
    {
        unchecked { return (X*397) ^ Y; }
    }

    public static bool operator ==(PointS left, PointS right) => left.Equals(right);
    public static bool operator !=(PointS left, PointS right) => !(left == right);

    public void Offset(short  x, short  y) {
		X += x;
		Y += y;
	}

	public void Set(short  x, short  y) {
		X = x;
		Y = y;
	}

	public override string ToString() {
		var culture = CultureInfo.CurrentCulture;
        return $"{{ X = {X.ToString(culture)}, Y = {Y.ToString(culture)} }}";
    }

	public bool IsEmpty => X == 0 && Y == 0;
}

[StructLayout(LayoutKind.Sequential)]
public struct PointF : IEquatable<PointF>
{
	public float X, Y;

	public PointF(float x, float y)
	{
		X = x;
		Y = y;
	}

	public bool Equals(PointF other) => X == other.X && Y == other.Y;
	public override bool Equals(object? obj) => obj is PointF f && Equals(f);

	public override int GetHashCode()
	{
		unchecked
		{
			return ((int)X * 397) ^ (int)Y;
		}
	}

	public static bool operator ==(PointF left, PointF right) => left.Equals(right);
	public static bool operator !=(PointF left, PointF right) => !(left == right);

	public void Offset(float x, float y)
	{
		X += x;
		Y += y;
	}

	public void Set(float x, float y)
	{
		X = x;
		Y = y;
	}

	public override string ToString()
	{
		var culture = CultureInfo.CurrentCulture;
		return $"{{ X = {X.ToString(culture)}, Y = {Y.ToString(culture)} }}";
	}

	public bool IsEmpty => X == 0 && Y == 0;
}