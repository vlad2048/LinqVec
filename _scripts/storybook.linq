<Query Kind="Program">
  <Reference>C:\dev\big\LinqVec\Libs\LinqVec\bin\Debug\net8.0-windows\LinqVec.dll</Reference>
  <Reference>C:\dev\big\LinqVec\LibsBase\LogLib\bin\Debug\net8.0\LogLib.dll</Reference>
  <Reference>C:\dev\big\LinqVec\LibsBase\ReactiveVars\bin\Debug\net8.0\ReactiveVars.dll</Reference>
  <Namespace>LinqVec</Namespace>
  <Namespace>LogLib.Writers</Namespace>
  <Namespace>LinqVec.Logging</Namespace>
  <Namespace>LinqVec.Utils.Json</Namespace>
  <Namespace>PowBasics.Json_</Namespace>
  <Namespace>LogLib.Interfaces</Namespace>
  <Namespace>LINQPad.Controls</Namespace>
  <Namespace>LogLib.Structs</Namespace>
  <Namespace>PowBasics.CollectionsExt</Namespace>
</Query>

public const string FileSrc = @"C:\tmp\vec\cons\chunks.json";
public const string FileDst = @"C:\tmp\vec\cons\chunks.html";

void Main()
{
	ExportToHtml(FileSrc, FileDst);

	/*var cols = (
		from gen in VecJsoner.Vec.Load<GenNfo<IWriteSer>[]>(FileSrc)
		from chunk in gen.Chunks
		where chunk is TextChunk
		select $"{((TextChunk)chunk).Seg.ColorName}"
	)
	.Distinct()
	.ToArray();
	cols.Dump();*/
}

static string GetColKey(string name)
{
	name = name.Replace('.', '_');
	var parts = name.Split('_');
	if (parts.Length <= 1) return string.Empty;
	return parts.SkipLast().JoinText("_");
}


public static void ExportToHtml(string fileSrc, string fileDst)
{
	var con = new LINQPadTxtWriter();
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
	Misc.SaveCurrentHtml(fileDst);
}

// @formatter:off
static class Misc {
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
	private static readonly Dictionary<string, VarNfo> varMap = new();
	private sealed record VarNfo(string VarName, string CssVarName, string VarVal);
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
		var grps = varMap.Values
			.GroupBy(e => GetColKey(e.VarName))
			.SelectToArray(e => e.OrderBy(f => f.VarName).ToArray());
		//grps.Dump();
		foreach (var grp in grps)
			WriteVarGroup(grp);
	}
	private static void WriteVarGroup(VarNfo[] vs)
	{
		var str = string.Join(Environment.NewLine, vs.Select(e => $$"""		--{{e.VarName}}: {{e.VarVal}};"""));
		Util.HtmlHead.AddStyles(
		$$"""
			:root {
		{{str}}
			}
		""");
	}
	static string GetColKey(string name)
	{
		name = name.Replace('.', '_');
		var parts = name.Split('_');
		if (parts.Length <= 1) return string.Empty;
		return parts.SkipLast().JoinText("_");
	}

	private static string SetJS(string varName, string varVal)
	{
		Util.HtmlHead.AddStyles($$""" :root { --{{varName}}: {{varVal}}; } """);
		return $"var(--{varName})";
	}
	private static VarNfo GetVarNfo(string varName, string varVal) => new(varName, $"var(--{varName})", varVal);
	private static string GetValName(string? expr)
	{
		if (string.IsNullOrEmpty(expr)) throw new ArgumentException();
		expr = expr.Replace(".", "_");
		return expr;
	}
}
void OnStart() { CssVars.Reset(); Util.HtmlHead.AddStyles("body { font-family: Consolas; }"); }
// @formatter:on
