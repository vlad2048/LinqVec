using System.Globalization;
using System.Runtime.InteropServices;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace WinAPI.NetCoreEx.Geometry;

[StructLayout(LayoutKind.Sequential)]
public struct Size : IEquatable<Size>
{
	public int Width;
	public int Height;

    public Size(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public bool Equals(Size other) => Width == other.Width && Height == other.Height;

    public override bool Equals(object? obj) => obj is Size size && Equals(size);

    public override int GetHashCode() => (Width * 397) ^ Height;

    public static bool operator ==(Size left, Size right) => left.Equals(right);
    public static bool operator !=(Size left, Size right) => !(left == right);

	public void Offset(int  width, int  height) {
		Width += width;
		Height += height;
	}

	public void Set(int  width, int  height) {
		Width = width;
		Height = height;
	}

	public override string ToString() {
		var culture = CultureInfo.CurrentCulture;
		return $"{{ Width = {Width.ToString(culture)}, Height = {Height.ToString(culture)} }}";
    }
	
	public bool IsEmpty => Width == 0 && Height == 0;
}

[StructLayout(LayoutKind.Sequential)]
public struct SizeS : IEquatable<SizeS>
{
	public short Width;
	public short Height;

    public SizeS(short width, short height)
    {
        Width = width;
        Height = height;
    }

    public bool Equals(SizeS other) => Width == other.Width && Height == other.Height;

    public override bool Equals(object? obj) => obj is SizeS s && Equals(s);

    public override int GetHashCode()
    {
        unchecked { return (Width * 397) ^ Height; }
    }

    public static bool operator ==(SizeS left, SizeS right) => left.Equals(right);
    public static bool operator !=(SizeS left, SizeS right) => !(left == right);

	public void Offset(short  width, short  height) {
		Width += width;
		Height += height;
	}

	public void Set(short  width, short  height) {
		Width = width;
		Height = height;
	}

	public override string ToString() {
		var culture = CultureInfo.CurrentCulture;
		return $"{{ Width = {Width.ToString(culture)}, Height = {Height.ToString(culture)} }}";
    }
	
	public bool IsEmpty => Width == 0 && Height == 0;
}

[StructLayout(LayoutKind.Sequential)]
public struct SizeF : IEquatable<SizeF>
{
	public float Width;
	public float Height;

	public SizeF(float width, float height)
	{
		Width = width;
		Height = height;
	}

	public bool Equals(SizeF other) => Width == other.Width && Height == other.Height;

	public override bool Equals(object? obj) => obj is SizeF f && Equals(f);

	public override int GetHashCode()
	{
		unchecked
		{
			return ((int)Width * 397) ^ (int)Height;
		}
	}

	public static bool operator ==(SizeF left, SizeF right) => left.Equals(right);
	public static bool operator !=(SizeF left, SizeF right) => !(left == right);

	public void Offset(float width, float height)
	{
		Width += width;
		Height += height;
	}

	public void Set(float width, float height)
	{
		Width = width;
		Height = height;
	}

	public override string ToString()
	{
		var culture = CultureInfo.CurrentCulture;
		return $"{{ Width = {Width.ToString(culture)}, Height = {Height.ToString(culture)} }}";
	}

	public bool IsEmpty => Width == 0 && Height == 0;
}