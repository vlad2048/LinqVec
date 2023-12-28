using System.Text.Json.Serialization;

namespace Geom;

public readonly record struct R
{
    public Pt Min { get; init; }
    public Pt Max { get; init; }
    [JsonIgnore]
	public float Width => Max.X - Min.X;
    [JsonIgnore]
	public float Height => Max.Y - Min.Y;
    public override string ToString() => $"{Min.X},{Min.Y} {Width}x{Height}";

    public R(Pt min, Pt max)
    {
        if (max.X < min.X || max.Y < min.Y) throw new ArgumentException("Invalid RGen");
        Min = min;
        Max = max;
    }

    public static readonly R Empty = default;

    public static R Make(float x, float y, float width, float height) => new(new Pt(x, y), new Pt(x + width, y + height));

    public bool Contains(Pt p) =>
	    p.X >= Min.X &&
	    p.Y >= Min.Y &&
	    p.X <= Max.X &&
	    p.Y <= Max.Y;

    public static R FromCenter(Pt center, float radius) => new(
        new Pt(center.X - radius, center.Y - radius),
        new Pt(center.X + radius, center.Y + radius)
    );
}