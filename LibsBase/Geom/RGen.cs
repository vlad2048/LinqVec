using System.Numerics;

namespace Geom;

public readonly record struct RGen<T> where T : struct, INumber<T>
{
    public PtGen<T> Min { get; }
    public PtGen<T> Max { get; }
    public T Width => Max.X - Min.X;
    public T Height => Max.Y - Min.Y;
    public override string ToString() => $"{Min.X},{Min.Y} {Width}x{Height}";

    public RGen(PtGen<T> min, PtGen<T> max)
    {
        if (max.X < min.X || max.Y < min.Y) throw new ArgumentException("Invalid RGen");
        Min = min;
        Max = max;
    }

    public static readonly RGen<T> Empty = new(new PtGen<T>(T.Zero, T.Zero), new PtGen<T>(T.Zero, T.Zero));

    public static RGen<T> Make(T x, T y, T width, T height) => new(new PtGen<T>(x, y), new PtGen<T>(x + width, y + height));

    public bool Contains(PtGen<T> p) =>
	    p.X >= Min.X &&
	    p.Y >= Min.Y &&
	    p.X <= Max.X &&
	    p.Y <= Max.Y;

    public static RGen<T> FromCenter(PtGen<T> center, T radius) => new(
        new PtGen<T>(center.X - radius, center.Y - radius),
        new PtGen<T>(center.X + radius, center.Y + radius)
    );
}

public static class RExt
{
	public static RGen<T> Enlarge<T>(this RGen<T> r, T radius) where T : struct, INumber<T> => new(
        new PtGen<T>(r.Min.X - radius, r.Min.Y - radius),
        new PtGen<T>(r.Max.X + radius, r.Max.Y + radius)
	);
}