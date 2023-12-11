using PowBasics.Json_;
using System.Text.Json;

namespace LinqVec.Utils.Json;

public static class VecJsoner
{
	private static readonly JsonSerializerOptions defaultJsonOpt = new()
	{
		WriteIndented = true,
	};

	public static readonly Jsoner Default = new(defaultJsonOpt);
}