using System.Text.Json.Serialization;

namespace Geom;

public readonly record struct Pt(float X, float Y)
{
    public static readonly Pt Zero = default;
    public override string ToString() => $"{X},{Y}";
    public static Pt operator +(Pt a, Pt b) => new(a.X + b.X, a.Y + b.Y);
    public static Pt operator -(Pt a, Pt b) => new(a.X - b.X, a.Y - b.Y);
    public static Pt operator -(Pt a) => new(-a.X, -a.Y);
    public static Pt operator *(Pt a, float f) => new(a.X * f, a.Y * f);
    [JsonIgnore]
    public float Length => MathF.Sqrt(X * X + Y * Y);

    public Pt Norm() => this * (1 / Length);
}