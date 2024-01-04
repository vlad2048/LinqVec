using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Kernel;
using LogLib.ConTickerLogic.Structs;
using PowBasics.CollectionsExt;
using ReactiveVars;
using Txt = LogLib.Structs.IChunk[];

namespace LogLib.ConTickerLogic.Logic;

public interface ISrc
{
	IObservable<Txt> WhenEvt { get; }
}

public sealed record VarSrc(IRoVar<Txt> Source) : ISrc
{
	public IObservable<Txt> WhenEvt => Obs.Never<Txt>();
}
public sealed record EvtSrc(IObservable<Txt> Source) : ISrc
{
	public IObservable<Txt> WhenEvt => Source;
}

sealed record SlotUnsortedInst(
	SlotNfo Nfo,
	ISrc Source
);

sealed record SlotInst(
	SlotLoc Loc,
	SlotNfo Nfo,
	ISrc Source
);

class SlotMan : IDisposable
{
	private const int Gutter = 1;

	private readonly Disp d;
	public void Dispose() => d.Dispose();

	private readonly IRwVar<SlotUnsortedInst[]> slots;

	public IRoVar<SlotInst[]> Slots { get; }

	public SlotMan(Disp d)
	{
		this.d = d;
		slots = Var.Make<SlotUnsortedInst[]>([], d);
		Slots = slots.Select(Sort).ToVar(d);
	}


	public void Add(SlotUnsortedInst slot, Disp slotD)
	{
		slots.V = slots.V.AddArr(slot);
		Disposable.Create(() =>
		{
			if (slots.D.IsDisposed) return;
			slots.V = slots.V.RemoveArr(slot);
		}).D(slotD);
	}


	private static SlotInst[] Sort(SlotUnsortedInst[] slots)
	{
		var orderedSlots = slots.OrderBy(e => e.Nfo.Priority).ToArray();
		return orderedSlots.Zip(ComputeLocs(orderedSlots))
			.SelectToArray(t => new SlotInst(t.Item2, t.Item1.Nfo, t.Item1.Source));
	}


	private static SlotLoc[] ComputeLocs(SlotUnsortedInst[] slots)
	{
		var list = new List<SlotLoc>();
		var x = 0;
		for (var i = 0; i < slots.Length; i++)
		{
			var slot = slots[i];
			var lng = (i == slots.Length - 1) switch {
				true => Console.WindowWidth,
				false => slot.Nfo.Size
			};
			list.Add(new SlotLoc(x, lng));
			x += slot.Nfo.Size + Gutter;
		}
		return list.ToArray();
	}
}




file static class EnumExt
{
	public static T[] AddArr<T>(this T[] xs, T x) => xs.ToList().Append(x).ToArray();

	public static T[] RemoveArr<T>(this T[] xs, T x)
	{
		var list = xs.ToList();
		if (!list.Remove(x)) throw new ArgumentException();
		return list.ToArray();
	}
}