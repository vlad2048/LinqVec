using System.Text.Json.Serialization;
using System.Text.Json;

namespace LinqVec.Utils.Json.Converters;

class OptionConverterFactory : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert) =>
		typeToConvert.IsGenericType &&
		typeToConvert.GetGenericTypeDefinition() == typeof(Option<>);

	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		var wrappedType = typeToConvert.GetGenericArguments()[0];
		var converter = (JsonConverter)Activator.CreateInstance(typeof(OptionConverter<>).MakeGenericType(wrappedType))!;
		return converter;
	}
}

sealed class OptionConverter<T> : JsonConverter<Option<T>>
{
	private sealed record Ser(bool HasValue, T V);
	private static Ser ToSer(Option<T> opt) => new(opt.IsSome, opt.IfNone(default(T)!));
	private static Option<T> FromSer(Ser ser) => ser.HasValue ? ser.V : None;

	public override Option<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		using var doc = JsonDocument.ParseValue(ref reader);
		return FromSer(doc.Deserialize<Ser>(options)!);
	}

	public override void Write(Utf8JsonWriter writer, Option<T> value, JsonSerializerOptions options) =>
		JsonSerializer.Serialize(writer, ToSer(value), options);
}