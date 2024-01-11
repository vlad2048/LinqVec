using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using DynamicData;
using LogLib.ConTickerLogic.Logic;
using LogLib.ConTickerLogic.Structs;
using PowBasics.CollectionsExt;
using ReactiveVars;
using Set = System.Collections.Generic.HashSet<string>;
using DynamicData.Kernel;
using LogLib.Structs;
using LogLib.Utils;
using LogLib.Writers;
using PowBasics.StringsExt;
using System.Reactive.Disposables;

namespace LogLib.ConTickerLogic;

static class LogTickerConstants
{
	public const string StorybookExe = @"C:\dev\big\LinqVec\Demos\Storybook\bin\Debug\net8.0-windows\Storybook.exe";

	public static readonly TimeSpan MaxTickLength = TimeSpan.FromMilliseconds(10);
	public const int Gutter = 1;
	public const int PrefixSizeEvent = 2;
}

public class LogTicker : IDisposable
{
	private readonly Disp d;
	public void Dispose() => d.Dispose();

    private readonly ISourceCache<SlotUnsortedInst, string> slots;
    private readonly LogTickerHistory history;
    private IRoVar<SlotInst[]> Slots { get; }
	private readonly Set tickFired = new();
	private bool HasFiredAndFlag(string name) => !tickFired.Add(name);
	private bool IsTickEmpty() => tickFired.Count == 0;


	private sealed record LogEvt(ItemWithValue<SlotInst, Txt> Item, bool NewTick);


	public LogTicker(
		IScheduler scheduler,
		IObservable<Unit> whenSave,
		Action<IChunk[], string> saveAction,
		IRoVar<TimeLogType> timeLogType,
		IRoVar<bool> disableLogTicker,
		Disp d
	)
	{
		S.Init();
		history = new LogTickerHistory(saveAction);
		this.d = d;
		slots = new SourceCache<SlotUnsortedInst, string>(e => e.Nfo.Name).D(d);
		Slots = slots.Connect()
			.FilterOnObservable(e => e.Nfo.WhenEnabled)
			.ToSortedCollection(e => e.Nfo.Priority)
			.Select(SlotArranger.Arrange)
			.ToVar(d);

		var timeTracker = new TimeTracker(scheduler, d);
		var slotTime = new SlotNfo(
			SlotType.Var,
			"time",
			Priority: -1,
			Size: 11,
			timeLogType.Select(e => e != TimeLogType.None).ObserveOn(scheduler)
		);
		Add(new SlotUnsortedInst(slotTime, new VarSrc(timeTracker.TimeRec.Select(t => t.RenderDeltaTime(timeLogType.V)).ToVar())), d);

		// New tick happens when:
		//		- slots are added/removed
		//		- a slot of type Event fires BUT it already fired during this tick
		//		- a slot of type Event fires BUT this tick is already too long
		Obs.Merge(
				Slots.Where(_ => !disableLogTicker.V).Select(_ => Option<LogEvt>.None),
				Slots
					.MergeManyItems(e => e.Source.WhenEvt)
					.Where(_ => !disableLogTicker.V)
					.Select(e => new LogEvt(e, timeTracker.IsTickTooLong() || HasFiredAndFlag(e.Item.Nfo.Name)))
					.Select(Some)
			)
			.Subscribe(tOpt =>
			{
				if (tOpt.Map(e => e.NewTick).IfNone(false))
				{
					tickFired.Clear();
					timeTracker.NewTick();
					history.NewTick();
					Console.WriteLine();
				}
				tOpt.IfSome(t => LogEvent(t.Item));
			}).D(d);


		whenSave.Subscribe(_ =>
		{
			var file = Path.GetTempFileName();
			history.Save(file);
			Process.Start(LogTickerConstants.StorybookExe, file);
		}).D(d);
	}

	public void Log(
		RenderNfo renderNfo,
		Disp slotD
	) =>
		Add(new SlotUnsortedInst(renderNfo.Nfo, renderNfo.Source), slotD);


	private void Add(SlotUnsortedInst slot, Disp slotD)
	{
		slots.AddOrUpdate(slot);
		Disposable.Create(() => slots.Remove(slot)).D(slotD);
	}

	private void LogEvent(ItemWithValue<SlotInst, IChunk[]> item)
	{
		ReactiveVarsLogger.EnsureMainThread();
		var txt = item.Value;
		var slot = item.Item;

		if (IsTickEmpty())
		{
			LogAllVars();
			//LogTitleBar();
		}

		tickFired.Add(slot.Nfo.Name);

		Render(txt, slot);
	}

	private void LogAllVars()
	{
		var varSlots = Slots.V.WhereToArray(e => e.Nfo.Type == SlotType.Var);
		foreach (var varSlot in varSlots)
		{
			var varVal = ((VarSrc)varSlot.Source).Source.V;
			Render(varVal, varSlot);
		}
	}


	private void Render(Txt txt, SlotInst slot)
	{
		var loc = slot.Loc;
		if (slot.Nfo.Type == SlotType.Event)
		{
			var sysLoc = loc with { Size = LogTickerConstants.PrefixSizeEvent };
			var sysTxt = new MemoryTxtWriter().Write($"{tickFired.Count}_", S.LogTicker.Cnt).Chunks;
			RenderFragment(sysTxt, sysLoc);
			loc = new SlotLoc(loc.Pos + LogTickerConstants.PrefixSizeEvent, loc.Size - LogTickerConstants.PrefixSizeEvent);
		}

		if (!ReactiveVarsLogger.AreWeOnTheMainThread) txt = txt.SetBack(S.LogTicker.BackAlert);
		RenderFragment(txt, loc);
	}

	private void RenderFragment(Txt txt, SlotLoc loc)
	{
		txt = txt.ClipToConsole(loc);
		txt.RenderToConsoleSlot(loc);
		history.RenderFragment(txt, loc);
	}


	private void LogTitleBar()
	{
		var (curX, curY) = (Console.CursorLeft, Console.CursorTop);
		var (fore, back) = (Console.ForegroundColor, Console.BackgroundColor);

		(Console.CursorLeft, Console.CursorTop) = (0, 0);
		ConUtils.SetFore(S.LogTicker.TitleBarFore);
		ConUtils.SetBack(S.LogTicker.TitleBarBack);
		Console.Write(new string(' ', Console.WindowWidth));
		foreach (var slot in Slots.V)
		{
			Console.CursorLeft = slot.Loc.Pos;
			Console.Write(slot.Nfo.Name.Truncate(slot.Loc.Size));
		}
		(Console.CursorLeft, Console.CursorTop) = (curX, curY);
		(Console.ForegroundColor, Console.BackgroundColor) = (fore, back);
	}
}
