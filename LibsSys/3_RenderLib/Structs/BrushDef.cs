namespace RenderLib.Structs;

public interface BrushDef { }
public sealed record SolidBrushDef(Color Color) : BrushDef;
public sealed record BmpBrushDef(Bitmap Bmp) : BrushDef;