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