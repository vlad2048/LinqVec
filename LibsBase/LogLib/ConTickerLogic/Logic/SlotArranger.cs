using LogLib.ConTickerLogic.Structs;
using PowBasics.CollectionsExt;

namespace LogLib.ConTickerLogic.Logic;

static class SlotArranger
{
	public static SlotInst[] Arrange(IReadOnlyCollection<SlotUnsortedInst> slots)
	{
		var orderedSlots = slots.OrderBy(e => e.Nfo.Priority).ToArray();
		return orderedSlots.Zip(ComputeSizes(orderedSlots))
			.SelectToArray(t => new SlotInst(t.Item2, t.Item1.Nfo, t.Item1.Source));
	}

	private static SlotLoc[] ComputeSizes(SlotUnsortedInst[] slots)
	{
		var sizes =
			slots
				.OrderBy(e => e.Nfo.Priority)
				.Select(e => new {
					e.Nfo.Size,
					IsEvent = e.Nfo.Type is SlotType.Event
				})
				.Select(t => t with { Size = t.Size + LogTickerConstants.Gutter + (t.IsEvent ? LogTickerConstants.PrefixSizeEvent : 0) })
				.Select(t => t.Size)
				.ToArray();

		var xs =
			sizes
				.Prepend(0)
				.Accumulate(0, (acc, lng) => acc + lng)
				.SkipLast()
				.ToArray();

		return
			sizes
				.Select(e => e - LogTickerConstants.Gutter)
				.Zip(xs)
				.Select(t => new SlotLoc(t.Item2, t.Item1))
				.ToArray();
	}

	private static IEnumerable<Acc> Accumulate<T, Acc>(this IEnumerable<T> source, Acc seed, Func<Acc, T, Acc> fun)
	{
		var accumulation = new List<Acc>();
		var current = seed;
		foreach (var item in source)
		{
			current = fun(current, item);
			accumulation.Add(current);
		}
		return accumulation;
	}
}