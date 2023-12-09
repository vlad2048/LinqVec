using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Geom.JsonConverters;

public class PtGenConverter<T> : JsonConverter<PtGen<T>> where T : struct, INumber<T>
{
	private sealed record R(T X, T Y);
	private static R ToR(PtGen<T> e) => new(e.X, e.Y);
	private static PtGen<T> FromR(R e) => new(e.X, e.Y);

	public override PtGen<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		using var doc = JsonDocument.ParseValue(ref reader);
		var r = doc.Deserialize<R>(options)!;
		return FromR(r);
	}

	public override void Write(Utf8JsonWriter writer, PtGen<T> value, JsonSerializerOptions options)
	{
		var r = ToR(value);
		JsonSerializer.Serialize(writer, r, options);
	}
}