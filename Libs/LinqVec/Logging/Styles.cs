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
using LogLib.ConTickerLogic;
using LogLib.ConTickerLogic.Logic;
using LogLib.ConTickerLogic.Structs;
using Txt = LogLib.Structs.IChunk[];
using LogLib.Writers;
using PtrLib;
using ReactiveVars;

namespace LinqVec.Logging;



// @formatter:off
public static class Styles
{
	// ***********
	// * Hotspot *
	// ***********
	public static readonly SlotNfo Slot_Hotspot = new(
		SlotType.Event,
		"hotspot",
		Priority: 1,
		Size: 10
	);

	internal static RenderNfo RenderHotspot(this IRoVar<Option<Hotspot>> source) =>
		source
			.Map2(e => e.HotspotNfo.Name)
			.Select(e => e.IfNone("(none)"))
			.ToVar()
			.PrettifyEvt((e, w) => w
				.Write(e)
			)
		.WithSlot(Slot_Hotspot);


	// **************
	// * IsDragging *
	// **************
	public static readonly SlotNfo Slot_IsDragging = new(
		SlotType.Event,
		"drag",
		Priority: 2,
		Size: 10
	);

	public static RenderNfo RenderFlag(this IRoVar<bool> source, SlotNfo slot) =>
		source.PrettifyEvt((e, w) => w
			.RenderFlag(e, slot.Name)
		)
		.WithSlot(slot);


	// *******
	// * Evt *
	// *******
	public static readonly SlotNfo Slot_Evt = new(
		SlotType.Event,
		"evt",
		Priority: 3,
		Size: 12
	);

	public static RenderNfo RenderEvt(this IObservable<IEvt> source) =>
		source.PrettifyEvt((e, w) => w
			.Write(() => e switch
			{
				MouseMoveEvt { Pos: var pos } => w.Write("Move").fmtPt(pos),
				MouseEnterEvt => w.Write("Enter"),
				MouseLeaveEvt => w.Write("Leave"),
				MouseBtnEvt { UpDown: var upDown, Btn: var btn } => w.fmtBtn(upDown, btn),
				MouseLeftBtnUpOutside => w.fmtLeftButtonUpOutside(),
				MouseClickEvt { Btn: var btn } => w.fmtClick(btn),
				MouseWheelEvt { Delta: var delta } => w.fmtWheel(delta),
				KeyEvt { Key: var key } => w.fmtKey(key),
				_ => throw new ArgumentException()
			})
			.SetDefaultFore(colEvt)
		)
		.WithSlot(Slot_Evt);

	private static readonly Col colEvt = new(0x65218a, nameof(colEvt));


	// ********
	// * IUsr *
	// ********
	public static readonly SlotNfo Slot_Usr = new(
		SlotType.Event,
		"usr",
		Priority: 4,
		Size: 11
	);

	internal static RenderNfo RenderUsr(this IObservable<IUsr> source) =>
		source.PrettifyEvt((e, w) => w
			.Write(() => e switch
			{
				MoveUsr {Pt: var pt} => w.Write("Move").fmtPt(pt),
				LDownUsr => w.fmtBtn(UpDown.Down, MouseBtn.Left),
				LUpUsr => w.fmtBtn(UpDown.Up, MouseBtn.Left),
				RDownUsr => w.fmtBtn(UpDown.Down, MouseBtn.Right),
				RUpUsr => w.fmtBtn(UpDown.Up, MouseBtn.Right),
				KeyDownUsr { Key: var key } => w.fmtKey(key),
				MouseLeftBtnUpOutsideUsr => w.fmtLeftButtonUpOutside(),
				_ => throw new ArgumentException()
			})
			.fmtQuick(e.IsQuick)
			.SetDefaultFore(colUsr)
		)
		.WithSlot(Slot_Usr);

	private static readonly Col colUsr = new(0xa635c3, nameof(colUsr));
	private static readonly Col colUsrFast = new(0x4e9b29, nameof(colUsrFast));
	private static W fmtQuick(this W w, bool quick) => quick ? w.Write("*", colUsrFast) : w;



	// ********
	// * ICmd *
	// ********
	public static readonly SlotNfo Slot_Cmd = new(
		SlotType.Event,
		"cmd",
		Priority: 5,
		Size: 10
	);

	internal static RenderNfo RenderCmd(this IObservable<ICmdEvt> source) =>
		source.PrettifyEvt((e, w) => w
			.Write(() => e switch
			{
				DragStartCmdEvt { HotspotCmd: var cmd } => w.Write("DragStart", colCmd_DragStart).PadRight(12).fmtCmd(cmd),
				DragFinishCmdEvt { HotspotCmd: var cmd } => w.Write("DragFinish", colCmd_DragFinish).PadRight(12).fmtCmd(cmd),
				ConfirmCmdEvt { HotspotCmd: var cmd } => w.Write("Confirm", colCmd_Confirm).PadRight(12).fmtCmd(cmd),
				ShortcutCmdEvt { ShortcutNfo.Key: var key } => w.fmtKey(key),
				CancelCmdEvt => w.Write("Cancel", colCmd_Cancel),
				_ => throw new ArgumentException()
			})
			.PadRight(16)
			.SetDefaultFore(colCmd)
		)
		.WithSlot(Slot_Cmd);

	private static readonly Col colCmd = new(0xf55ff0, nameof(colCmd));
	private static readonly Col colCmd_DragStart = new(0xe6e835, nameof(colCmd_DragStart));
	private static readonly Col colCmd_DragFinish = new(0x3be341, nameof(colCmd_DragFinish));
	private static readonly Col colCmd_Confirm = new(0x3be341, nameof(colCmd_Confirm));
	private static readonly Col colCmd_Shortcut = new(0x2786cf, nameof(Shortcut));
	private static readonly Col colCmd_Cancel = new(0xf55067, nameof(colCmd_Cancel));
	
	private static W fmtCmd(this W w, IHotspotCmd cmd) => w; //.Write($"{cmd.Gesture}->{cmd.Name}");


	// ********
	// * IMod *
	// ********
	public static readonly SlotNfo Slot_Mod = new(
		SlotType.Event,
		"mod",
		Priority: 6,
		Size: 16
	);

	public static RenderNfo RenderMod(this IObservable<IModEvt> source) =>
		source.PrettifyEvt((e, w) => w
			.Write(() => e switch {
				ModStartEvt { Name: var name }
					=> w.Blk(b => b
						.Write(name, colMod_Name)
						.Surround("Start(", ")", colMod_Start)
				),
				ModFinishEvt { Name: var name, Commit: var commit, Str: var str }
					=> w.Blk(b => b
						.Write(name, colMod_Name)
						.Surround(commit ? "Commit(" : "Cancel(", ")", commit ? colMod_Commit : colMod_Cancel)
					)
					.spc(8)
					.Write(str.Truncate(32), colMod_Model),
				_ => throw new ArgumentException()
			})
		)
		.WithSlot(Slot_Mod);

	private static readonly Col colMod_Start = new(0xbbe04c, nameof(colMod_Start));
	private static readonly Col colMod_Commit = new(0x4ce659, nameof(colMod_Commit));
	private static readonly Col colMod_Cancel = new(0xe8554a, nameof(colMod_Cancel));
	private static readonly Col colMod_Name = new(0x4dbbeb, nameof(colMod_Name));
	private static readonly Col colMod_Model = new(0x4b6069, nameof(colMod_Model));







	private static IRoVar<Txt> PrettifyVar<T>(
		this IRoVar<T> source,
		Action<T, W> fmt
	) => 
		source.Select(e =>
		{
			var w = new MemoryTxtWriter();
			fmt(e, w);
			return w.Chunks;
		})
		.ToVar();

	private static RenderNfo WithSlot(this IRoVar<Txt> source, SlotNfo slot) => new(new VarSrc(source), slot);

	private static IObservable<Txt> PrettifyEvt<T>(
		this IObservable<T> source,
		Action<T, W> fmt
	) =>
		source.Select(e =>
		{
			var w = new MemoryTxtWriter();
			fmt(e, w);
			return w.Chunks;
		});

	private static RenderNfo WithSlot(this IObservable<Txt> source, SlotNfo slot) => new(new EvtSrc(source), slot);






	// **********
	// * Shared *
	// **********
	private static readonly Col gen_colKey = new(0x0f6e0f, nameof(gen_colKey));
	private static readonly Col gen_colTime = new(0xbf9822, nameof(gen_colTime));

	// Mouse
	// =====
	private static readonly Col colDown = new(0x28affc, nameof(colDown));
	private static readonly Col colUp = new(0x757575, nameof(colUp));
	private static readonly Col colUpOutsideBack = new(0xeb5f73, nameof(colUpOutsideBack));
	private static readonly Col colUpOutsideFore = new(0x41efe8, nameof(colUpOutsideFore));
	private static Col col(this UpDown upDown) => upDown is UpDown.Down ? colDown : colUp;
	private static W fmtBtn(this W w, UpDown upDown, MouseBtn btn) => w
		.Write($"{btn}", upDown.col())
		.Surround('(', ')', B.gen_colNeutral);
	private static W fmtLeftButtonUpOutside(this W w) => w
		.Write("Up-Outside", colUpOutsideFore, colUpOutsideBack);
	private static W fmtClick(this W w, MouseBtn btn) => w
		.Write($"{btn}", UpDown.Down.col())
		.Surround("Click(", ")", B.gen_colNeutral);
	private static W fmtWheel(this W w, int delta) => w
		.Write(delta > 0 ? "+" : "-", gen_colKey)
		.Surround("Wheel(", ")", B.gen_colNeutral);

	// Key
	// ===
	private static W fmtKey(this W w, Keys key) => w
		.Write($"{key}", gen_colKey)
		.Surround("Key(", ")", B.gen_colNeutral);

	// Pt
	// ==
	private static readonly Col colPt = new(0x303030, nameof(colPt));
	private static W fmtPt(this W w, Pt pt) => w.Write($"@{(int)pt.X},{(int)pt.Y}", colPt).PadRight(8);
}
// @formatter:on
