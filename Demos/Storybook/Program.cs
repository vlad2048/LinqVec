using System.Windows.Forms;
using Geom;
using LinqVec.Logging;
using LinqVec.Tools.Cmds.Enums;
using LinqVec.Tools.Cmds.Events;
using LinqVec.Tools.Cmds.Logic;
using LinqVec.Tools.Cmds.Structs;
using LinqVec.Tools.Events;
using LinqVec.Utils.Json;
using LogLib.Writers;

namespace Storybook;

static class Program
{
	private const string FileOut = @"C:\tmp\vec\cons\chunks.json";
	private static readonly ConWriter<IWriteSer> con = ConWriter<IWriteSer>.Instance;

	static void Main()
	{
		Render();
		con.Save(VecJsoner.Vec, FileOut);
	}


	private static void Render()
	{
		foreach (var element in elements)
			con.Gen(element);
	}

	private static readonly Pt p0 = new(-2, 10);
	private static readonly Pt p1 = new(2, 3);
	private static readonly Pt p2 = new(-5, 2);
	private static readonly Pt p3 = new(-4, 5);
	private static readonly Pt p4 = new(7, -8);
	private static readonly IHotspotCmd hotspotCmd = new DragHotspotCmd("CmdName", Gesture.DoubleClick, (_, _) => _ => {});

	private static readonly IWriteSer[] elements =
	{
		new MouseMoveEvt(p0),
		new MouseEnterEvt(),
		new MouseLeaveEvt(),
		new MouseBtnEvt(p1, UpDown.Down, MouseBtn.Left, ModKeyState.Empty),
		new MouseBtnEvt(p1, UpDown.Up, MouseBtn.Left, ModKeyState.Empty),
		new MouseBtnEvt(p1, UpDown.Down, MouseBtn.Right, ModKeyState.Empty),
		new MouseBtnEvt(p1, UpDown.Up, MouseBtn.Right, ModKeyState.Empty),
		new MouseBtnEvt(p1, UpDown.Down, MouseBtn.Middle, ModKeyState.Empty),
		new MouseBtnEvt(p1, UpDown.Up, MouseBtn.Middle, ModKeyState.Empty),
		new MouseWheelEvt(p2, +1),
		new MouseWheelEvt(p2, -1),
		new KeyEvt(UpDown.Down, Keys.S),
		new KeyEvt(UpDown.Up, Keys.S),

		new MoveUsr(false, p3),
		new MoveUsr(true, p3),
		new LDownUsr(false, p3, ModKeyState.Empty),
		new LDownUsr(true, p3, ModKeyState.Empty),
		new RDownUsr(false, p3),
		new RDownUsr(true, p3),
		new KeyDownUsr(false, Keys.S),
		new KeyDownUsr(true, Keys.S),

		new DragStartCmdEvt(hotspotCmd, p4),
		new DragFinishCmdEvt(hotspotCmd, p0, p1),
		new ConfirmCmdEvt(hotspotCmd, p2),
		new ShortcutCmdEvt(new ShortcutNfo("ShortcutAction", Keys.T, () => {})),
		new CancelCmdEvt(),
	};
}