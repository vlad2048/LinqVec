using Vortice.Mathematics;
using Color = System.Drawing.Color;

namespace RenderLib.Utils.DirectX.Exts;

static class ConversionExts
{
    public static Color4 ToColor(this Color c) => new(c.R, c.G, c.B, c.A);
}