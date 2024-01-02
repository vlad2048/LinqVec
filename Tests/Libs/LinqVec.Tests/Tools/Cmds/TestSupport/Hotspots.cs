using LinqVec.Tools.Cmds.Utils;

namespace LinqVec.Tests.Tools.Cmds.TestSupport;

static class Hotspots
{
	public static readonly HotspotNfo<Unit> Left = new(
		nameof(Left),
		p => p.X < 0 ? Unit.Default : None
	);

	public static readonly HotspotNfo<Unit> Right = new(
		nameof(Right),
		p => p.X >= 0 ? Unit.Default : None
	);
}