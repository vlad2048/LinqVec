/*

	**********************************************************************
	**********************************************************************
	****															   ****
	****	This file is designed to be copy/pasted as is in LINQPad   ****
	****	to tweak the logging design easily.						   ****
	****															   ****
	**********************************************************************
	**********************************************************************

1) Copy and paste this file in LINQPad

2) Link to these 3 DLLs: LogLib.dll, LinqVec.Dll & ReactiveVars.Dll
   They are in these folders:
		C:\dev\big\LinqVec\LibsBase\LogLib\bin\Debug\net8.0
		C:\dev\big\LinqVec\Libs\LinqVec\bin\Debug\net8.0-windows
		C:\dev\big\LinqVec\LibsBase\ReactiveVars\bin\Debug\net8.0

3) Uncomment the LINQPad section below

4) Run it

*/

using LinqVec.Logging;
using LinqVec.Logging.Utils;
using LinqVec.Tools.Cmds.Events;
using LinqVec.Tools.Cmds.Logic;
using LinqVec.Tools.Events;
using LogLib;
using LogLib.Interfaces;
using LogLib.Structs;




/*

//	*******************************************
//	*******************************************
//	****                                   ****
//	****  LINQPad Section: Uncomment this  ****
//	****                                   ****
//	*******************************************
//	*******************************************


using LINQPad.Controls;
using LinqVec.Utils.Json;
using PowBasics.Json_;
using LogLib.Writers;

void Main()
{
	Print_With_CustomCodeInLINQPad_ForEasyTweaking();
	//Print_With_OriginalLinkedCode();
}

public static void Print_With_CustomCodeInLINQPad_ForEasyTweaking()
{
	var file = @"C:\tmp\vec\cons\init.json";
	var con = new LINQPadTxtWriter();
	var writeFun = LogVecConKeeper.ReplayWithCustomFunctions<IWriteSer>(typeof(Stylesheet), file);
	writeFun(con);
}
public static void Print_With_OriginalLinkedCode()
{
	var gens = VecJsoner.Vec.Load<GenNfo<IWriteSer>[]>(@"C:\tmp\vec\cons\init.json");
	var con = new LINQPadTxtWriter();
	foreach (var gen in gens)
		gen.Src.Write(con);
}




// @formatter:off
sealed class LINQPadTxtWriter : ITxtWriter
{
	private readonly List<Span> curLine = new();
	public int LastSegLength { get; private set; }
	public ITxtWriter Write(TxtSegment seg) { curLine.Add(MkSpan(seg)); LastSegLength = seg.Text.Length; return this; }
	public ITxtWriter WriteLine() { FlushCurLine(); LastSegLength = 0; return this; }
	private void FlushCurLine() { var div = MkDiv(curLine.ToArray()); curLine.Clear(); div.Dump(); }
	private static Span MkSpan(TxtSegment seg) { var span = new Span(seg.Text); span.Styles["color"] = $"#{(seg.Color & 0xFF0000) >> 16:X2}{(seg.Color & 0xFF00) >> 8:X2}{(seg.Color & 0xFF) >> 0:X2}"; return span; }
	private static Div MkDiv(Span[] spans) { var div = new Div(spans); return div; }
}
void OnStart() => Util.HtmlHead.AddStyles("body { font-family: Consolas; }");
// @formatter:on


//	*******************************************
//	****      End of LINQPad Section       ****
//	*******************************************
   
*/





// ReSharper disable once CheckNamespace
static class LinqVecColors
{
	public const int General_Gray = 0x6e6e6e;
	public const int General_LightGray = 0xa8a8a8;

	public const int Hotspot_Name = 0xe8e8e8;
	public const int Hotspot_Cmd = 0xd47be8;
	public const int Hotspot_Shortcut = 0xe89f4d;

	public const int Evt_Key = 0x6a5773;
	public const int Evt_MouseMove = 0x4d17cc;
	public const int Evt_Others = 0xa584b5;
	public const int Evt_Down = 0x39db49;
	public const int Evt_Up = 0xcc354c;
	public const int Evt_Btn = 0x969696;

	public const int UsrEvt_Key = 0x3964b7;
	public const int UsrEvt_Move = 0x273e7d;
	public const int UsrEvt_Others = 0x6b8eed;
	public const int UsrEvt_Fast = 0xcee649;

	public const int CmdEvt_Key = 0xe94eb7;
	public const int CmdEvt_DragStart = 0xe86b35;
	public const int CmdEvt_DragFinish = 0x3be341;
	public const int CmdEvt_Confirm = 0x3be341;
	public const int CmdEvt_Shortcut = 0x2786cf;
	public const int CmdEvt_Cancel = 0xdb213a;
}



static class Stylesheet
{
	public static ITxtWriter Color(this IEvt Evt, ITxtWriter w) => w
		.Write("[    evt]", LinqVecColors.Evt_Key)
		.Space(1)
		.Write(Evt.GetBaseName("Evt"), Evt switch
		{
			MouseMoveEvt => LinqVecColors.Evt_MouseMove,
			_ => LinqVecColors.Evt_Others
		}).Pad(11)
		.IfType<IEvt, MouseBtnEvt>(Evt, f => w
			.Write("btn:", LinqVecColors.General_Gray)
			.Write($"{f.Btn}", LinqVecColors.Evt_Btn)
			.Space(1)
			.Write(f.UpDown is UpDown.Down ? new TxtSegment("down", LinqVecColors.Evt_Down) : new TxtSegment("up", LinqVecColors.Evt_Up))
		)
		.IfType<IEvt, MouseClickEvt>(Evt, g => w
			.Write("btn:", LinqVecColors.General_Gray)
			.Write($"{g.Btn}", LinqVecColors.Evt_Btn)
		)
		.IfType<IEvt, KeyEvt>(Evt, g => w.Key(g.Key))
		.WriteLine();


	public static ITxtWriter Color(this IUsr UsrEvt, ITxtWriter w) => w
		.Write("[usr-evt]", LinqVecColors.UsrEvt_Key)
		.Space(1)
		.Space(32)
		.Write(UsrEvt.GetBaseName("Usr"), UsrEvt switch
		{
			MoveUsr => LinqVecColors.UsrEvt_Move,
			_ => LinqVecColors.UsrEvt_Others
		})
		.WriteIf(UsrEvt.IsQuick, "(f)", LinqVecColors.UsrEvt_Fast)
		.WriteLine();


	public static ITxtWriter Color(this ICmdEvt CmdEvt, ITxtWriter w) => w
		.Write("[cmd-evt]", LinqVecColors.CmdEvt_Key)
		.Space(1)
		.Space(64)
		.Write(() => CmdEvt switch {
			DragStartCmdEvt => w.Write("DragStart", LinqVecColors.CmdEvt_DragStart),
			DragFinishCmdEvt => w.Write("DragFinish", LinqVecColors.CmdEvt_DragFinish),
			ConfirmCmdEvt => w.Write("Confirm", LinqVecColors.CmdEvt_Confirm),
			ShortcutCmdEvt { ShortcutNfo.Key: var key } => w.Write("Shortcut", LinqVecColors.CmdEvt_Shortcut).Space(1).Key(key),
			CancelCmdEvt => w.Write("Cancel", LinqVecColors.CmdEvt_Cancel),
			_ => throw new ArgumentException()
		})
		.Pad(16)
		.Write(() => CmdEvt switch {
			IIHotspotCmdEvt { HotspotCmd: var cmd } => w.Cmd(cmd.Name),
			ShortcutCmdEvt { ShortcutNfo: var shortcut } => w.Shortcut(shortcut.Name),
			CancelCmdEvt => w,
			_ => throw new ArgumentException()
		})
		.WriteLine();




	public static ITxtWriter Color(this TimestampCon e, ITxtWriter w) => w
		.WriteTime(e.Time);

	public static ITxtWriter Color(this IsHotspotFrozenCon e, ITxtWriter w) => w
		.WriteFlag("frozen", e.Flag);

	public static ITxtWriter Color(this HotspotNameCon e, ITxtWriter w) => w
		.Write("name:", LinqVecColors.General_LightGray)
		.WriteIf(e.Name == "_", "_", LogLibColors.Off)
		.WriteIf(e.Name != "_", e.Name, LogLibColors.Gray);
}



