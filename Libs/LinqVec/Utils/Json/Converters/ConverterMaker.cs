using System.Text.Json;
using System.Text.Json.Serialization;

namespace LinqVec.Utils.Json.Converters;

static class ConverterMaker
{
	public static readonly JsonConverter<Color> ColorConverter = MakeConverter<Color, ColorSer>(e => new(e.A, e.R, e.G, e.B), e => Color.FromArgb(e.A, e.R, e.G, e.B));
	private sealed record ColorSer(byte A, byte R, byte G, byte B);



	private static JsonConverter<T> MakeConverter<T, S>(Func<T, S> serFun, Func<S, T> deserFun) => new CallbackConverter<T, S>(serFun, deserFun);


	private sealed class CallbackConverter<T, S>(Func<T, S> serFun, Func<S, T> deserFun) : JsonConverter<T>
	{
		public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			using var doc = JsonDocument.ParseValue(ref reader);
			return deserFun(doc.Deserialize<S>(options)!);
		}

		public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
			JsonSerializer.Serialize(writer, serFun(value), options);
	}
}