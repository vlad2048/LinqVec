<Query Kind="Program">
  <Reference>C:\dev\big\LinqVec\Libs\LinqVec\bin\Debug\net8.0-windows\LinqVec.dll</Reference>
  <Reference>C:\dev\big\LinqVec\LibsBase\LogLib\bin\Debug\net8.0\LogLib.dll</Reference>
  <Reference>C:\dev\big\LinqVec\LibsBase\ReactiveVars\bin\Debug\net8.0\ReactiveVars.dll</Reference>
  <Namespace>LinqVec</Namespace>
</Query>

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

3) Rename
	MyLinqVecColors to MyLinqVecColors
	MyStylesheet to MyStylesheet

4) Run it

*/

using LinqVec.Logging;
using LinqVec.Tools.Cmds.Events;
using LinqVec.Tools.Cmds.Logic;
using LinqVec.Tools.Events;
using LogLib;
using LogLib.Interfaces;
using LogLib.Structs;
#if NETCORE
using LINQPad.Controls;
using LinqVec.Utils.Json;
using PowBasics.Json_;
using LogLib.Writers;
#endif



// ReSharper disable once CheckNamespace
public static class MyLinqVecColors
{
	public static int General_DarkGray = 0x207341;
	public static int General_Gray = 0x6e6e6e;
	public static int General_LightGray = 0xa8a8a8;
	public static int General_Key = 0x0f6e0f;
	public static int General_Cmd = 0x488cbd;
	public static int General_Down = 0x1b5866;
	public static int General_Up = 0x2c8396;
	public static int General_Time = 0xbf9822;
	public static int General_On = 0x5fba32;
	public static int General_Off = 0x418520;

	public static int Hotspot_None = 0x1b7074;
	public static int Hotspot_Some = 0x0faad1;


	public static int Evt_Key = 0x65218a;

	public static int Usr_Key = 0xa635c3;
	public static int Usr_Fast = 0x4e9b29;

	public static int Cmd_Key = 0xf55ff0;
	public static int Cmd_DragStart = 0xe86b35;
	public static int Cmd_DragFinish = 0x3be341;
	public static int Cmd_Confirm = 0x3be341;
	public static int Cmd_Shortcut = 0x2786cf;
	public static int Cmd_Cancel = 0xdb213a;
}
#if NETCORE


//	*******************************************
//	*******************************************
//	****                                   ****
//	****  LINQPad Section: Uncomment this  ****
//	****                                   ****
//	*******************************************
//	*******************************************




void Main()
{
	var fileSrc = @"C:\tmp\vec\cons\init.json";
	var fileDst = fileSrc.ChangeFileExtension(".html");
	var con = new LINQPadTxtWriter();

	Print_With_CustomCodeInLINQPad_ForEasyTweaking(fileSrc, con);
	//Print_All_Chunks(fileSrc, con);
	//Print_With_OriginalLinkedCode(fileSrc, con);

	//Print_VarNames(fileSrc);

	Misc.SaveCurrentHtml(fileDst);
}


public static void Print_All_Chunks(string fileSrc, ITxtWriter con)
{
	var chunks = (
		from gen in VecJsoner.Vec.Load<GenNfo<IWriteSer>[]>(fileSrc)
		from chunk in gen.Chunks
		select chunk
	).ToArray();
	foreach (var chunk in chunks)
	{
		switch (chunk)
		{
			case TextChunk { Seg: var seg }:
				con.Write(seg);
				break;
			case NewlineChunk:
				con.WriteLine();
				break;
			default:
				throw new ArgumentException();
		}
	}
}

public static void Print_With_CustomCodeInLINQPad_ForEasyTweaking(string fileSrc, ITxtWriter con)
{
	var writeFun = LogVecConKeeper.ReplayWithCustomFunctions<IWriteSer>(typeof(MyStylesheet), fileSrc);
	//var gens = VecJsoner.Vec.Load<GenNfo<IWriteSer>[]>(fileSrc);
	//var writeFun = LogVecConKeeper.ReplayWithCustomFunctions<IWriteSer>(typeof(MyStylesheet), gens);

	writeFun(con);
}

public static void Print_With_OriginalLinkedCode(string fileSrc, ITxtWriter con)
{
	var gens = VecJsoner.Vec.Load<GenNfo<IWriteSer>[]>(fileSrc);
	foreach (var gen in gens)
		gen.Src.Write(con);
}

public static void Print_VarNames(string fileSrc) => (
		from gen in VecJsoner.Vec.Load<GenNfo<IWriteSer>[]>(fileSrc)
		from chunk in gen.Chunks
		where chunk is TextChunk
		select $"{((TextChunk)chunk).Seg.ColorName}"
	)
	.Distinct()
	.Dump();




// @formatter:off
static class Misc {
	public static string ChangeFileExtension(this string fileSrc, string extDst) => Path.Combine(Path.GetDirectoryName(fileSrc)!, $"{Path.GetFileNameWithoutExtension(fileSrc)}{extDst}");
	public static void SaveCurrentHtml(string file) { CssVars.Flush(); File.WriteAllText(file, Util.InvokeScript(true, "eval", "document.documentElement.outerHTML") as string); }
}
sealed class LINQPadTxtWriter : ITxtWriter
{
	private readonly List<Span> curLine = new();
	public int LastSegLength { get; private set; }
	public int AbsoluteX { get; private set; }
	public ITxtWriter Write(TxtSegment seg) { curLine.Add(MkSpan(seg)); LastSegLength = seg.Text.Length; AbsoluteX += seg.Text.Length; return this; }
	public ITxtWriter WriteLine() { FlushCurLine(); LastSegLength = 0; AbsoluteX = 0; return this; }
	private void FlushCurLine() { var div = MkDiv(curLine.ToArray()); curLine.Clear(); div.Dump(); }
	private static Span MkSpan(TxtSegment seg) { var span = new Span(seg.Text); span.Styles["color"] = CssVars.Set(seg.ColorName ?? throw new ArgumentException(), Val2HexColor(seg.Color)); return span; }
	private static Div MkDiv(Span[] spans) { var div = new Div(spans); return div; }
	public static string Val2HexColor(int val) => $"#{(val & 0xFF0000) >> 16:X2}{(val & 0xFF00) >> 8:X2}{(val & 0xFF) >> 0:X2}";
}
public static class CssVars
{
	private sealed record VarNfo(string CssVarName, string VarVal);
	public static void Reset() => varMap.Clear();	
	public static string Set(string varName, string varVal)
	{
		varName = GetValName(varName);
		if (!varMap.TryGetValue(varName, out var varNfo))
			varNfo = varMap[varName] = GetVarNfo(varName, varVal);
		return varNfo.CssVarName;
	}
	public static void Flush()
	{
		var str = string.Join(Environment.NewLine, varMap.Select(e => $$"""		--{{e.Key}}: {{e.Value.VarVal}};"""));
		Util.HtmlHead.AddStyles(
		$$"""
			:root {
		{{str}}
			}
		""");
	}

	private static readonly Dictionary<string, VarNfo> varMap = new();
	private static string SetJS(string varName, string varVal)
	{
		Util.HtmlHead.AddStyles($$""" :root { --{{varName}}: {{varVal}}; } """);
		return $"var(--{varName})";
	}
	private static VarNfo GetVarNfo(string varName, string varVal) => new($"var(--{varName})", varVal);
	private static string GetValName(string? expr)
	{
		if (string.IsNullOrEmpty(expr)) throw new ArgumentException();
		expr = expr.Replace(".", "_");
		return expr;
	}
}
void OnStart() { CssVars.Reset(); Util.HtmlHead.AddStyles("body { font-family: Consolas; }"); }
// @formatter:on


//	*******************************************
//	****      End of LINQPad Section       ****
//	*******************************************


#endif






static class MyStylesheet
{
	private static ITxtWriter Surround(this ITxtWriter w, string strPrefix, string strInner, VName colInner, VName colOuter) => w
		.Write($"{strPrefix}(", colOuter)
		.Write(strInner, colInner)
		.Write(")", colOuter);
	private static VName EvtColor(this UpDown upDown) => upDown is UpDown.Down ? MyLinqVecColors.General_Down.ToVName() : MyLinqVecColors.General_Up.ToVName();
	public static ITxtWriter CmdC(this ITxtWriter w, string cmd) => w.Surround("cmd", cmd, MyLinqVecColors.General_Cmd.ToVName(), MyLinqVecColors.General_Gray.ToVName());

	public static ITxtWriter Color(this IEvt Evt, ITxtWriter w) => w
		.Write("[evt]", MyLinqVecColors.Evt_Key)
		.Space(4)

		.Write(() => Evt switch
		{
			MouseMoveEvt => w.Write("Move", MyLinqVecColors.Evt_Key),
			MouseEnterEvt => w.Write("Enter", MyLinqVecColors.Evt_Key),
			MouseLeaveEvt => w.Write("Leave", MyLinqVecColors.Evt_Key),
			MouseBtnEvt { UpDown: var upDown, Btn: var btn } => w.Surround("Btn", $"{btn}", upDown.EvtColor(), MyLinqVecColors.Evt_Key.ToVName()),
			MouseClickEvt { Btn: var btn } => w.Surround("Click", $"{btn}", UpDown.Up.EvtColor(), MyLinqVecColors.Evt_Key.ToVName()),
			MouseWheelEvt => w.Write("Wheel", MyLinqVecColors.Evt_Key),
			KeyEvt { Key: var key } => w.Surround("Key", $"{key}", MyLinqVecColors.General_Key.ToVName(), MyLinqVecColors.Evt_Key.ToVName()),
			_ => throw new ArgumentException()
		})
		.WriteLine();


	private static ITxtWriter UsrQuick(this ITxtWriter w, IUsr Usr) => Usr.IsQuick switch
	{
		true => w.Write("*", MyLinqVecColors.Usr_Fast),
		false => w.Space(1)
	};
	private static ITxtWriter UsrRegular(this ITxtWriter w, IUsr Usr, string str) => w
		.Write(str, MyLinqVecColors.Usr_Key).UsrQuick(Usr);
	private static ITxtWriter UsrSurround(this ITxtWriter w, IUsr Usr, string prefix, string str, VName innerColor) => w.Surround(prefix, str, innerColor, MyLinqVecColors.Usr_Key.ToVName()).UsrQuick(Usr);
	public static ITxtWriter Color(this IUsr Usr, ITxtWriter w) => w
		.Write("[usr]", MyLinqVecColors.Usr_Key)
		.Write("      └─────────➜", MyLinqVecColors.General_Gray)
		.Write(() => Usr switch
		{
			MoveUsr => w.UsrRegular(Usr, "Move"),
			LDownUsr => w.UsrSurround(Usr, "Btn", "Left", UpDown.Down.EvtColor()),
			LUpUsr => w.UsrSurround(Usr, "Btn", "Left", UpDown.Up.EvtColor()),
			RDownUsr => w.UsrSurround(Usr, "Btn", "Right", UpDown.Down.EvtColor()),
			RUpUsr => w.UsrSurround(Usr, "Btn", "Right", UpDown.Up.EvtColor()),
			KeyDownUsr { Key: var key } => w.UsrSurround(Usr, "Key", $"{key}", MyLinqVecColors.General_Key.ToVName()),
			_ => throw new ArgumentException()
		})
		.WriteLine();


	public static ITxtWriter Color(this ICmdEvt CmdEvt, ITxtWriter w) => w
		.Write("[cmd]", MyLinqVecColors.Cmd_Key)
		.Write("                    └─────────➜", MyLinqVecColors.General_Gray)
		.Write(() => CmdEvt switch
		{
			DragStartCmdEvt e => w.Write("DragStart", MyLinqVecColors.Cmd_DragStart).PadAbs(48).CmdC($"[{e.HotspotCmd.Gesture}]->{e.HotspotCmd.Name}"),
			DragFinishCmdEvt e => w.Write("DragFinish", MyLinqVecColors.Cmd_DragFinish).PadAbs(48).CmdC($"[{e.HotspotCmd.Gesture}]->{e.HotspotCmd.Name}"),
			ConfirmCmdEvt e => w.Write("Confirm", MyLinqVecColors.Cmd_Confirm).PadAbs(48).CmdC($"[{e.HotspotCmd.Gesture}]->{e.HotspotCmd.Name}"),
			ShortcutCmdEvt { ShortcutNfo.Key: var key } => w.Surround("Shortcut", $"{key}", MyLinqVecColors.General_Key.ToVName(), MyLinqVecColors.Cmd_Shortcut.ToVName()),
			CancelCmdEvt => w.Write("Cancel", MyLinqVecColors.Cmd_Cancel),
			_ => throw new ArgumentException()
		})
		//.Pad(16)
		.WriteLine();




	public static ITxtWriter Color(this TimestampCon e, ITxtWriter w) => w
		.WriteTime(e.Time);

	public static ITxtWriter Color(this IsHotspotFrozenCon e, ITxtWriter w) => w
		.WriteFlag("frozen", e.Flag);

	public static ITxtWriter Color(this HotspotNameCon e, ITxtWriter w) => w
		.Write("name: ", MyLinqVecColors.General_LightGray)
		.Write(() => e.Name switch
		{
			"_" => w.Write("(none)", MyLinqVecColors.Hotspot_None),
			_ => w.Write(e.Name, MyLinqVecColors.Hotspot_Some)
		})
		.WriteLine();





	private static ITxtWriter WriteTime(this ITxtWriter w, DateTimeOffset t) => w
		.Write($"[{t:HH:mm:ss.fffffff}]", MyLinqVecColors.General_Time)
		.Space(1);

	private static ITxtWriter WriteFlag(this ITxtWriter w, string? name, bool val) => w
		.WriteIf(name != null, $"[{name}:", MyLinqVecColors.General_DarkGray)
		.Write(val ? new TxtSegment("on ", MyLinqVecColors.General_On) : new TxtSegment("off", MyLinqVecColors.General_Off))
		.Write("]", MyLinqVecColors.General_DarkGray)
		.Space(1);

}



