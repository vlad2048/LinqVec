using PowBasics.StringsExt;

namespace LinqVec.Utils;

static class StringExt
{
	public static string Indent(this string s, int indent) =>
		s
			.SplitInLines()
			.Select(e => new string(' ', indent) + e)
			.JoinLines();
}