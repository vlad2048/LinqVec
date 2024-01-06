using System.Diagnostics;
using System.Drawing;
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
	private static readonly SlotNfo Slot_DeltaTime = new(
		SlotType.Var,
		"time",
		Priority: -1,
		Size: 9,
		Obs.Return(true)
	);

	private readonly Disp d;
	public void Dispose() => d.Dispose();

    private readonly Func<DateTime> time;
    private readonly ISourceCache<SlotUnsortedInst, string> slots;
    private readonly LogTickerHistory history;
    private IRoVar<SlotInst[]> Slots { get; }
	private readonly Set tickFired = new();
	private bool HasFiredAndFlag(string name) => !tickFired.Add(name);
	private DateTime tickStart;
	private bool IsTickTooLong() => time() - tickStart > LogTickerConstants.MaxTickLength;
	private bool IsTickEmpty() => tickFired.Count == 0;


	private sealed record LogEvt(ItemWithValue<SlotInst, Txt> Item, bool NewTick);


	public LogTicker(
		IScheduler scheduler,
		IObservable<Unit> whenSave,
		Action<IChunk[], string> saveAction,
		Disp d
	)
	{
		S.Init();
		history = new LogTickerHistory(saveAction);
		time = () => scheduler.Now.DateTime;
		this.d = d;
		var tickDelta = Var.Make(TimeSpan.Zero, d);
		slots = new SourceCache<SlotUnsortedInst, string>(e => e.Nfo.Name).D(d);
		Slots = slots.Connect()
			.FilterOnObservable(e => e.Nfo.WhenEnabled)
			.ToSortedCollection(e => e.Nfo.Priority)
			.Select(SlotArranger.Arrange)
			.ToVar(d);

		Add(new SlotUnsortedInst(Slot_DeltaTime, new VarSrc(tickDelta.Select(t => t.RenderDeltaTime()).ToVar())), d);

		// New tick happens when:
		//		- slots are added/removed
		//		- a slot of type Event fires BUT it already fired during this tick
		//		- a slot of type Event fires BUT this tick is already too long
		Obs.Merge(
				Slots.Select(_ => Option<LogEvt>.None),
				Slots
					.MergeManyItems(e => e.Source.WhenEvt)
					.Select(e => new LogEvt(e, IsTickTooLong() || HasFiredAndFlag(e.Item.Nfo.Name)))
					.Where(e => e.NewTick)
					.Select(Some)
			)
			.Subscribe(tOpt =>
			{
				if (tOpt.Map(e => e.NewTick).IfNone(false))
				{
					tickFired.Clear();
					var now = time();
					tickDelta.V = now - tickStart;
					tickStart = now;
					history.NewTick();
					Console.WriteLine();
				}
				tOpt.IfSome(t => LogEvent(t.Item));
			}).D(d);


		whenSave.Subscribe(_ =>
		{
			var file = Path.GetTempFileName();
			file = @"C:\tmp\vec\storybook\storybook.json";
			history.Save(file);
			Process.Start(LogTickerConstants.StorybookExe, file);
			disabled = true;
		}).D(d);
	}

	private bool disabled = false;

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
		if (disabled) return;
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
