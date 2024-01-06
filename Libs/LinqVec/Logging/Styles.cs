using System.Linq.Expressions;
using System.Reactive.Linq;
using Geom;
using LinqVec.Tools.Cmds.Logic;
using LinqVec.Tools.Events;
using LogLib.Structs;
using LogLib.Utils;
using LinqVec.Tools.Cmds.Events;
using LinqVec.Tools.Cmds.Structs;
using LinqVec.Utils.Rx;
using PowBasics.StringsExt;
using W = LogLib.Writers.ITxtWriter;
using LogLib;
using LogLib.ConTickerLogic.Structs;
using Txt = LogLib.Structs.IChunk[];
using LogLib.Writers;
using PowBasics.QueryExpr_;
using PtrLib;
using ReactiveVars;

namespace LinqVec.Logging;



public static class Styles
{
	// ***********
	// * Hotspot *
	// ***********
	public static readonly SlotNfo Slot_Hotspot = MkSlot(
		SlotType.Event,
		priority: 1,
		size: 10,
		cfg => cfg.Log.LogCmd.Hotspot
	);

	internal static RenderNfo RenderHotspot(this IRoVar<Option<Hotspot>> source) =>
		source
			.Map2IfNone(e => e.HotspotNfo.Name, "(none)")
			.Prettify((e, w) => w
				.Write(e)
			)
			.WithSlot(Slot_Hotspot);


	// **************
	// * IsDragging *
	// **************
	public static readonly SlotNfo Slot_IsDragging = MkSlot(
		SlotType.Event,
		priority: 2,
		size: 10,
		cfg => cfg.Log.LogCmd.Drag
	);

	public static RenderNfo RenderFlag(this IRoVar<bool> source, SlotNfo slot) =>
		source.Prettify((e, w) => w
				.RenderFlag(e, slot.Name)
			)
			.WithSlot(slot);


	// *******
	// * Evt *
	// *******
	public static readonly SlotNfo Slot_Evt = MkSlot(
		SlotType.Event,
		priority: 3,
		size: 12,
		cfg => cfg.Log.LogCmd.Evt
	);


	internal static RenderNfo RenderEvt(this IObservable<IEvt> source) => source.Select(RenderEvt).WithSlot(Slot_Evt);

	internal static Txt RenderEvt(this IEvt e) => W
		.Write(w => e switch {
			// @formatter:off
			MouseMoveEvt { Pos: var pos }						=> w.Write("Move").fmtPt(pos),
			MouseEnterEvt										=> w.Write("Enter"),
			MouseLeaveEvt										=> w.Write("Leave"),
			MouseBtnEvt { UpDown: var upDown, Btn: var btn }	=> w.fmtBtn(upDown, btn),
			MouseLeftBtnUpOutside								=> w.fmtLeftButtonUpOutside(),
			MouseClickEvt { Btn: var btn }						=> w.fmtClick(btn),
			MouseWheelEvt { Delta: var delta }					=> w.fmtWheel(delta),
			KeyEvt { Key: var key }								=> w.fmtKey(key),
			_													=> throw new ArgumentException()
			// @formatter:on
		})
		.Chunks.SetForeIfNull(S.Evt.Main);


	// ********
	// * IUsr *
	// ********
	public static readonly SlotNfo Slot_Usr = MkSlot(
		SlotType.Event,
		priority: 4,
		size: 11,
		cfg => cfg.Log.LogCmd.Usr
	);

	internal static RenderNfo RenderUsr(this IObservable<IUsr> source) => source.Select(RenderUsr).WithSlot(Slot_Usr);

	internal static Txt RenderUsr(this IUsr e) => W
		.Write(w => e switch {
			// @formatter:off
			MoveUsr { Pt: var pt}			=> w.Write("Move").fmtPt(pt),
			LDownUsr						=> w.fmtBtn(UpDown.Down, MouseBtn.Left),
			LUpUsr							=> w.fmtBtn(UpDown.Up, MouseBtn.Left),
			RDownUsr						=> w.fmtBtn(UpDown.Down, MouseBtn.Right),
			RUpUsr							=> w.fmtBtn(UpDown.Up, MouseBtn.Right),
			KeyDownUsr { Key: var key }		=> w.fmtKey(key),
			MouseLeftBtnUpOutsideUsr		=> w.fmtLeftButtonUpOutside(),
			_								=> throw new ArgumentException()
			// @formatter:on
		})
		.fmtQuick(e.IsQuick)
		.Chunks.SetForeIfNull(S.Usr.Main);

	private static W fmtQuick(this W w, bool quick) => quick ? w.Write("*", S.Usr.Fast) : w;



	// ********
	// * ICmd *
	// ********
	public static readonly SlotNfo Slot_Cmd = MkSlot(
		SlotType.Event,
		priority: 5,
		size: 10,
		cfg => cfg.Log.LogCmd.Cmd
	);
	internal static RenderNfo RenderCmd(this IObservable<ICmdEvt> source) => source.Select(RenderCmd).WithSlot(Slot_Cmd);

	internal static Txt RenderCmd(this ICmdEvt e) => W
		.Write(w => e switch {
			// @formatter:off
			DragStartCmdEvt { HotspotCmd: var cmd }			=> w.Write("DragStart", S.Cmd.DragStart).PadRight(12).fmtCmd(cmd),
			DragFinishCmdEvt { HotspotCmd: var cmd }		=> w.Write("DragFinish", S.Cmd.DragFinish).PadRight(12).fmtCmd(cmd),
			ConfirmCmdEvt { HotspotCmd: var cmd }			=> w.Write("Confirm", S.Cmd.Confirm).PadRight(12).fmtCmd(cmd),
			ShortcutCmdEvt { ShortcutNfo.Key: var key }		=> w.fmtKey(key),
			CancelCmdEvt									=> w.Write("Cancel", S.Cmd.Cancel),
			_												=> throw new ArgumentException()
			// @formatter:on
		})
		.PadRight(16)
		.Chunks.SetForeIfNull(S.Cmd.Main);

	private static W fmtCmd(this W w, IHotspotCmd cmd) => w; //.Write($"{cmd.Gesture}->{cmd.Name}");


	// ********
	// * IMod *
	// ********
	public static readonly SlotNfo Slot_Mod = MkSlot(
		SlotType.Event,
		priority: 6,
		size: 32,
		cfg => cfg.Log.LogCmd.Mod
	);
	public static RenderNfo RenderMod(this IObservable<IModEvt> source) => source.Select(RenderMod).WithSlot(Slot_Mod);

	internal static Txt RenderMod(this IModEvt e) => W
		.Write(w => e switch {
			ModStartEvt { Name: var name }
				=> w.Blk(b => b
					.Write(name, S.Mod.Name)
					.Surround("Start(", ")", S.Mod.Start)
				),
			ModFinishEvt { Name: var name, Commit: var commit, Str: var str }
				=> w.Blk(b => b
						.Write(name, S.Mod.Name)
						.Surround(commit ? "Commit(" : "Cancel(", ")", commit ? S.Mod.Commit : S.Mod.Cancel)
					),
					//.spc(8)
					//.Write(str.Truncate(32), S.Mod.Model),
			_ => throw new ArgumentException()
		}).Chunks;



	// **********
	// * Shared *
	// **********

	// Mouse
	// =====
	private static NamedColor col(this UpDown upDown) => upDown is UpDown.Down ? S.Mouse.Down : S.Mouse.Up;

	private static W fmtBtn(this W w, UpDown upDown, MouseBtn btn) => w
		.Write($"{btn}", upDown.col())
		.Surround('(', ')', S.General.Neutral);

	private static W fmtLeftButtonUpOutside(this W w) => w
		.Write("Up-Outside", S.Mouse.OutsideFore, S.Mouse.OutsideBack);

	private static W fmtClick(this W w, MouseBtn btn) => w
		.Write($"{btn}", UpDown.Down.col())
		.Surround("Click(", ")", S.General.Neutral);

	private static W fmtWheel(this W w, int delta) => w
		.Write(delta > 0 ? "+" : "-", S.Misc.Keys)
		.Surround("Wheel(", ")", S.General.Neutral);

	// Key
	// ===
	private static W fmtKey(this W w, Keys key) => w
		.Write($"{key}", S.Misc.Keys)
		.Surround("Key(", ")", S.General.Neutral);

	// Pt
	// ==
	private static W fmtPt(this W w, Pt pt) => w.Write($"@{(int)pt.X},{(int)pt.Y}", S.Misc.MousePos).PadRight(8);





	// *********
	// * Utils *
	// *********
	private static W W => new MemoryTxtWriter();

	private static SlotNfo MkSlot(
		SlotType type,
		int priority,
		int size,
		Expression<Func<Cfg, bool>> enabled
	)
	{
		var (get, _, name) = QueryExprUtils.RetrieveGetSetName(enabled);
		return new SlotNfo(
			type,
			name,
			priority,
			size,
			G.Cfg.When(get).ObserveOnUI()
		);
	}


	private static IObservable<Txt> Prettify<T>(
		this IObservable<T> source,
		Action<T, W> fmt
	) =>
		source.Select(e =>
		{
			var w = new MemoryTxtWriter();
			fmt(e, w);
			return w.Chunks;
		});

	private static RenderNfo WithSlot(this IObservable<Txt> source, SlotNfo slot) => new(
		slot.Type switch
		{
			SlotType.Event => new EvtSrc(source),
			SlotType.Var => new VarSrc(source.ToVar()),
			_ => throw new ArgumentException()
		},
		slot
	);
}
