using System.Windows.Forms;
using Geom;
using LinqVec.Tools.Cmds.Enums;
using LinqVec.Tools.Cmds.Events;
using LinqVec.Tools.Cmds.Logic;
using LinqVec.Tools.Cmds.Structs;
using LinqVec.Tools.Events;
using LinqVec.Utils.Json;
using LogLib.Writers;
using LogLib.Utils;
using PowBasics.Json_;
using LogLib;

namespace Storybook;

static class Program
{
	private const string FileOut = @"C:\tmp\vec\cons\chunks.json";

	static void Main()
	{
		/*var totalWriter = new MemoryTxtWriter();
		foreach (var element in elements)
		{
			var elementWriter = new MemoryTxtWriter();
			elementWriter.Write(element);
			totalWriter.WriteLine(elementWriter);
		}
		totalWriter.Chunks.RenderToConsole();
		VecJsoner.Vec.Save(FileOut, totalWriter.Chunks);*/
	}


	/*private static readonly Pt p0 = new(-10, -10);
	private static readonly Pt p1 = new(2, 3);
	private static readonly Pt p2 = new(-5, 2);
	private static readonly Pt p3 = new(-4, 5);
	private static readonly Pt p4 = new(7, -8);
	private static readonly IHotspotCmd hotspotCmd = new DragHotspotCmd("CmdName", Gesture.DoubleClick, (_, _) => _ => {});
	private static readonly IPretty Newline = new NewlinePretty();

	private static readonly IPretty[] elements =
	{
		new Header("Evt"),
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
		Newline,

		new Header("Usr"),
		new MoveUsr(false, p3),
		new MoveUsr(true, p3),
		new LDownUsr(false, p3, ModKeyState.Empty),
		new LDownUsr(true, p3, ModKeyState.Empty),
		new RDownUsr(false, p3),
		new RDownUsr(true, p3),
		new KeyDownUsr(false, Keys.S),
		new KeyDownUsr(true, Keys.S),
		Newline,

		new Header("Cmd"),
		new DragStartCmdEvt(hotspotCmd, p4),
		new DragFinishCmdEvt(hotspotCmd, p0, p1),
		new ConfirmCmdEvt(hotspotCmd, p2),
		new ShortcutCmdEvt(new ShortcutNfo("ShortcutAction", Keys.T, () => {})),
		new CancelCmdEvt(),
	};

	private sealed record Header(string Title) : IPretty
	{
		public void Write(ITxtWriter w) => w
			.WriteLine($"** {Title} **")
			.Write(new string('=', Title.Length + 6))
			.SetDefaultFore(B.gen_colWhite);
	}

	private sealed record NewlinePretty : IPretty
	{
		public void Write(ITxtWriter w) => w.WriteLine();
	}*/
}