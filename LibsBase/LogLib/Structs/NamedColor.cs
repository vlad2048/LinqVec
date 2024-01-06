using System.Drawing;

namespace LogLib.Structs;

public sealed record NamedColor(Color Color)
{
    public string Name { get; set; } = null!;
    public static implicit operator Color(NamedColor e) => e.Color;
}