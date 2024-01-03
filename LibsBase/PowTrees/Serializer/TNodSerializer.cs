using System.Text.Json;
using System.Text.Json.Serialization;

namespace PowTrees.Serializer;

public sealed class TNodSerializer<T> : JsonConverter<TNod<T>>
{
	private sealed class JsonNod
	{
		public T V { get; }
		public IReadOnlyList<JsonNod> Children { get; }

		public JsonNod(T v, IReadOnlyList<JsonNod> children)
		{
			V = v;
			Children = children;
		}
	}

	private static JsonNod ToJsonNod(TNod<T> root)
	{
		JsonNod ToJsonNodInner(TNod<T> node) => new(
			node.V,
			node.Children.Select(ToJsonNodInner).ToList()
		);

		return ToJsonNodInner(root);
	}

	private static TNod<T> FromJsonNod(JsonNod root) => Nod.Make(
		root.V,
		root.Children.Select(FromJsonNod)
	);

	public override TNod<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		using var doc = JsonDocument.ParseValue(ref reader);
		var serR = doc.Deserialize<JsonNod>(options)!;
		var r = FromJsonNod(serR);
		return r;
	}

	public override void Write(Utf8JsonWriter writer, TNod<T> value, JsonSerializerOptions options)
	{
		var serR = ToJsonNod(value);
		JsonSerializer.Serialize(writer, serR, options);
	}
}