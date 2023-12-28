using LinqVec.Tools.Cmds;

namespace LinqVec.Tests.Tools.Cmds.TestSupport;

static class Hotspots
{
	public static readonly Hotspot<Unit> Left = new(
		nameof(Left),
		p => p.X < 0 ? Unit.Default : None
	);

	public static readonly Hotspot<Unit> Right = new(
		nameof(Right),
		p => p.X >= 0 ? Unit.Default : None
	);
}