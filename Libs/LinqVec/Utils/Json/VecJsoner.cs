using LinqVec.Utils.Json.Converters;
using PowBasics.Json_;
using System.Text.Json;

namespace LinqVec.Utils.Json;

public static class VecJsoner
{
	private static readonly JsonSerializerOptions defaultJsonOpt = new()
	{
		WriteIndented = true,
		AllowTrailingCommas = true,
		ReadCommentHandling = JsonCommentHandling.Skip,
	};

	static VecJsoner()
	{
		defaultJsonOpt.Converters.Add(new OptionConverterFactory());
	}

	public static readonly Jsoner Default = new(defaultJsonOpt);
}