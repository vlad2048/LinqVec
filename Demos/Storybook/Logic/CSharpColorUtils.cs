namespace Storybook.Logic;

sealed record ColorNfo(
	string Name,
	Color Color,
	int LineIndex
);

static class CSharpColorUtils
{
	public static void Save(string file, ColorNfo[] colors)
	{
		var lines = File.ReadAllLines(file);
		foreach (var color in colors)
		{
			var lineIdx = color.LineIndex;
			var linePrev = lines[lineIdx];
			var lineNext = ChangeColor(linePrev, color.Color);
			lines[lineIdx] = lineNext;
		}
		File.WriteAllLines(file, lines);
	}

	public static ColorNfo[] Load(string file)
	{
		var list = new List<ColorNfo>();

		var lines = File.ReadAllLines(file);
		var stack = new Stack<string>();

		for (var i = 0; i < lines.Length; i++)
		{
			var line = lines[i];
			var clsName = IsPush(line);
			if (clsName != null)
			{
				stack.Push(clsName);
				continue;
			}
			if (IsPop(line))
			{
				if (stack.Count == 0) return list.ToArray();
				stack.Pop();
				continue;
			}
			var rec = IsColor(line);
			if (rec != null)
			{
				var color = new ColorNfo(
					string.Join(".", stack.Reverse().Append(rec.Name)),
					rec.Color,
					i
				);
				list.Add(color);
			}
		}

		return list.ToArray();
	}


	private const string PushPrefix = "public static class ";
	private static string? IsPush(string line)
	{
		line = line.Trim();
		if (!line.StartsWith(PushPrefix)) return null;
		return line[PushPrefix.Length..];
	}

	private const string PopPrefix = "}";
	private static bool IsPop(string line)
	{
		line = line.Trim();
		return line == PopPrefix;
	}

	private sealed record Rec(string Name, Color Color);
	private const string ColorPrefix = "public static readonly NamedColor ";
	private static Rec? IsColor(string line)
	{
		line = line.Trim();
		if (!line.StartsWith(ColorPrefix)) return null;
		var str = line[ColorPrefix.Length..];
		var parts = str.Split(new[] { ' ', '=', '(', ')' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		if (parts.Length < 3) return null;
		var colStr = parts[3];
		var col = Convert.ToInt32(colStr, 16);
		return new Rec(parts[0], Color.FromArgb(col));
	}

	private static string ChangeColor(string line, Color c)
	{
		var i0 = line.IndexOf("0x");
		var i1 = line.IndexOf(')', i0);
		var str = line[i0..i1];
		return line.Replace(str, $"0x{c.R:x2}{c.G:x2}{c.B:x2}");
	}
}