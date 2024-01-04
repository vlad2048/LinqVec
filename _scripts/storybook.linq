<Query Kind="Program">
  <Reference>C:\dev\big\LinqVec\Libs\LinqVec\bin\Debug\net8.0-windows\LinqVec.dll</Reference>
  <Reference>C:\dev\big\LinqVec\LibsBase\LogLib\bin\Debug\net8.0\LogLib.dll</Reference>
  <Reference>C:\dev\big\LinqVec\LibsBase\ReactiveVars\bin\Debug\net8.0\ReactiveVars.dll</Reference>
  <Namespace>LINQPad.Controls</Namespace>
  <Namespace>LogLib.Writers</Namespace>
  <Namespace>PowBasics.CollectionsExt</Namespace>
  <Namespace>LogLib.Structs</Namespace>
  <Namespace>LinqVec.Utils.Json</Namespace>
  <Namespace>PowBasics.Json_</Namespace>
</Query>

public const string FileSrc = @"C:\tmp\vec\cons\chunks.json";
public const string FileDst = @"C:\tmp\vec\cons\chunks.html";

void Main(string[] args)
{
	VecJsoner.Vec.Load<IChunk[]>(FileSrc)
		.Render()
		.SaveHtml(FileDst);
}


public static class SaveHtmlExt
{
	public static IEnumerable<IChunk> SaveHtml(this IEnumerable<IChunk> chunks, string file)
	{
		#if CMD
		return chunks;
		#endif
		CssVars.Flush();
		File.WriteAllText(file, Util.InvokeScript(true, "eval", "document.documentElement.outerHTML") as string);		
		return chunks;
	}
}

public static class LINQPadRenderExt
{
	public static IEnumerable<IChunk> Render(this IEnumerable<IChunk> chunks)
	{
		var curLine = new List<Span>();
		
		void Flush()
		{
			if (curLine.Count == 0) return;
			var div = new Div(curLine);
			div.Dump();
			curLine.Clear();
		}
		
		foreach (var chunk in chunks)
		{
			switch (chunk)
			{
				case TextChunk { Text: var text, Fore: var fore, Back: var back }:
					var span = new Span(text);
					if (fore != null) span.Styles["color"] = CssVars.Set(fore.Name, Val2HexColor(fore));
					if (back != null) span.Styles["background-color"] = CssVars.Set(back.Name, Val2HexColor(back));
					span.Styles["font-family"] = "Consolas";
					curLine.Add(span);
					break;
				case NewlineChunk:
					Flush();
					break;
				default:
					throw new ArgumentException();
			}
		}
		Flush();
		return chunks;
	}
	
	private static string Val2HexColor(Col col) => $"#{(col.Color & 0xFF0000) >> 16:X2}{(col.Color & 0xFF00) >> 8:X2}{(col.Color & 0xFF) >> 0:X2}";
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
