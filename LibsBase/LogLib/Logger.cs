using System.Drawing;
using LogLib.Utils;

namespace LogLib;

public static class Logger
{
	public static void Write(string s) => Console.Write(s);
	public static void WriteLine(string s) => Console.WriteLine(s);

	public static void Write(string s, Color col)
	{
		ConUtils.SetColor(col);
		Console.Write(s);
		ResetColor();
	}
	public static void Write(string s, int col) => Write(s, MkCol(col));

	public static void WriteLine(string s, Color col)
	{
		ConUtils.SetColor(col);
		Console.Write(s);
		ResetColor();
	}
	public static void WriteLine(string s, int col) => WriteLine(s, MkCol(col));
	public static void WriteLine() => Console.WriteLine();


	private static readonly Color DefaultColor = MkCol(0xCCCCCC);
	private static void ResetColor() => ConUtils.SetColor(DefaultColor);
	private static Color MkCol(uint v) => Color.FromArgb(0xFF, Color.FromArgb((int)v));
	private static Color MkCol(int v) => Color.FromArgb(0xFF, Color.FromArgb(v));
}