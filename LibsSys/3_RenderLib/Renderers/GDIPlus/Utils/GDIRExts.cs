namespace RenderLib.Renderers.GDIPlus.Utils;

static class GDIRExts
{
	public static R ReduceDimsByOne(this R r) => new(
		r.X,
		r.Y,
		Math.Max(0, r.Width - 1),
		Math.Max(0, r.Height - 1)
	);
}