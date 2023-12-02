using System.Numerics;

namespace Geom;

public readonly record struct PtGen<T>(T X, T Y) where T : struct, INumber<T>
{
    public static readonly PtGen<T> Zero = new(T.Zero, T.Zero);
    public override string ToString() => $"{X},{Y}";
    public static PtGen<T> operator +(PtGen<T> a, PtGen<T> b) => new(a.X + b.X, a.Y + b.Y);
    public static PtGen<T> operator -(PtGen<T> a, PtGen<T> b) => new(a.X - b.X, a.Y - b.Y);
    public static PtGen<T> operator -(PtGen<T> a) => new(-a.X, -a.Y);
    public static PtGen<T> operator *(PtGen<T> a, T f) => new(a.X * f, a.Y * f);
    public double Length => Math.Sqrt(Convert.ToDouble(X * X + Y * Y));

    public PtGen<T> Norm() => this * T.CreateChecked(1 / Length);
}