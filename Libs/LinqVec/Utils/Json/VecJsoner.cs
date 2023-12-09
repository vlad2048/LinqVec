using PowBasics.Json_;
using System.Text.Json;
using System.Text.Json.Serialization;
using DynamicData;
using Geom.JsonConverters;

namespace LinqVec.Utils.Json;

public static class VecJsoner
{
	private static readonly JsonSerializerOptions defaultJsonOpt = new()
	{
		WriteIndented = true,
	};

	static VecJsoner()
	{
		defaultJsonOpt.Converters.AddRange(new JsonConverter[]
		{
			new PtGenConverter<int>(),
			new PtGenConverter<float>(),
			new RGenConverter<int>(),
			new RGenConverter<float>(),
		});
	}

	public static readonly Jsoner Default = new(defaultJsonOpt);
}