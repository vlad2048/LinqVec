using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Geom.JsonConverters;

public class RGenConverter<T> : JsonConverter<RGen<T>> where T : struct, INumber<T>
{
	private sealed record R(T MinX, T MinY, T MaxX, T MaxY);
	private static R ToR(RGen<T> e) => new(e.Min.X, e.Min.Y, e.Max.X, e.Max.Y);
	private static RGen<T> FromR(R e) => new(new PtGen<T>(e.MinX, e.MinY), new PtGen<T>(e.MaxX, e.MaxY));

	public override RGen<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		using var doc = JsonDocument.ParseValue(ref reader);
		var r = doc.Deserialize<R>(options)!;
		return FromR(r);
	}

	public override void Write(Utf8JsonWriter writer, RGen<T> value, JsonSerializerOptions options)
	{
		var r = ToR(value);
		JsonSerializer.Serialize(writer, r, options);
	}
}