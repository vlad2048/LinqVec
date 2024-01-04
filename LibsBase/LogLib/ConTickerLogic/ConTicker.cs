using System.Drawing;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using LogLib.ConTickerLogic.Logic;
using LogLib.ConTickerLogic.Structs;
using PowBasics.CollectionsExt;
using ReactiveVars;
using Txt = LogLib.Structs.IChunk[];
using Set = System.Collections.Generic.HashSet<string>;
using DynamicData.Kernel;
using LogLib.Structs;
using LogLib.Utils;
using PowBasics.StringsExt;

namespace LogLib.ConTickerLogic;



public sealed record RenderNfo(ISrc Source, SlotNfo Nfo);

public class ConTicker : IDisposable
{
	private static readonly TimeSpan MaxTickLength = TimeSpan.FromMilliseconds(10);
	private static readonly SlotNfo Slot_DeltaTime = new(
		SlotType.Var,
		"time",
		Priority: -1,
		Size: 9
	);

	private readonly Disp d;
	public void Dispose() => d.Dispose();

    private readonly Func<DateTime> time;
	private readonly SlotMan man;
	private readonly Set tickFired = new();
	private readonly IRwVar<TimeSpan> tickDelta;
	private DateTime tickStart;
	private bool IsTickTooLong() => time() - tickStart > MaxTickLength;
	private bool IsTickEmpty() => tickFired.Count == 0;

	public ConTicker(IScheduler scheduler, Disp d)
	{
		time = () => scheduler.Now.DateTime;
		this.d = d;
		man = new SlotMan(d);
		tickDelta = Var.Make(TimeSpan.Zero, d);
		man.Add(new SlotUnsortedInst(Slot_DeltaTime, new VarSrc(tickDelta.Select(t => t.RenderDeltaTime()).ToVar())), d);

		// New tick happens when:
		//		- slots are added/removed
		//		- a slot of type Event fires BUT it already fired during this tick
		//		- a slot of type Event fires BUT this tick is already too long
		Obs.Merge(
				man.Slots.Select(_ => Option<ItemWithValue<SlotInst, Txt>>.None),
				man.Slots
					.MergeManyItems(e => e.Source.WhenEvt)
					.Where(t => tickFired.Contains(t.Item.Nfo.Name) || IsTickTooLong())
					.Select(Some)
			)
			.Subscribe(tOpt =>
			{
				tickFired.Clear();
				var now = time();
				tickDelta.V = now - tickStart;
				tickStart = now;
				Console.WriteLine();
				tOpt.IfSome(t => LogEvent(t.Item, t.Value));
			}).D(d);

		man.Slots.MergeManyItems(e => e.Source.WhenEvt)
			.Subscribe(t => LogEvent(t.Item, t.Value)).D(d);
	}


	public void FancyLog(
		RenderNfo renderNfo,
		Disp slotD
	) => man.Add(new SlotUnsortedInst(renderNfo.Nfo, renderNfo.Source), slotD);


	private int cnt;

	private void LogEvent(SlotInst slot, Txt valEvt)
	{
		if (cnt != 0)
			throw new ArgumentException();
		cnt++;
		ReactiveVarsLogger.EnsureMainThread();
		if (IsTickEmpty())
		{
			LogAllVars();
			//LogTitleBar();
		}
		tickFired.Add(slot.Nfo.Name);
		valEvt.RenderToSlot(slot.Loc);

		Console.CursorLeft = slot.Loc.Pos;
		ConUtils.SetFore(Color.White);
		Console.Write($"{tickFired.Count}");

		cnt--;
	}

	private void LogAllVars()
	{
		var varSlots = man.Slots.V.WhereToArray(e => e.Nfo.Type == SlotType.Var);
		foreach (var varSlot in varSlots)
		{
			var varVal = ((VarSrc)varSlot.Source).Source.V;
			varVal.RenderToSlot(varSlot.Loc);
		}
	}


	private static readonly Col col_TitleBarFore = new(0xb0b0b0, nameof(col_TitleBarFore));
	private static readonly Col col_TitleBarBack = new(0x303030, nameof(col_TitleBarBack));

	private void LogTitleBar()
	{
		var (curX, curY) = (Console.CursorLeft, Console.CursorTop);
		var (fore, back) = (Console.ForegroundColor, Console.BackgroundColor);

		(Console.CursorLeft, Console.CursorTop) = (0, 0);
		ConUtils.SetFore(MkCol(col_TitleBarFore.Color));
		ConUtils.SetBack(MkCol(col_TitleBarBack.Color));
		Console.Write(new string(' ', Console.WindowWidth));
		foreach (var slot in man.Slots.V)
		{
			Console.CursorLeft = slot.Loc.Pos;
			Console.Write(slot.Nfo.Name.Truncate(slot.Loc.Size));
		}

		(Console.CursorLeft, Console.CursorTop) = (curX, curY);
		(Console.ForegroundColor, Console.BackgroundColor) = (fore, back);
	}
}
