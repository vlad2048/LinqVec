using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace LogLib.Structs;

[JsonDerivedType(typeof(TextChunk), typeDiscriminator: "TextChunk")]
[JsonDerivedType(typeof(NewlineChunk), typeDiscriminator: "NewlineChunk")]
public interface IChunk
{
	[JsonIgnore]
	int Length { get; }
}

public sealed record TextChunk(string Text, Col? Fore = null, Col? Back = null) : IChunk
{
	[JsonIgnore]
	public int Length => Text.Length;
}

public sealed record NewlineChunk : IChunk
{
	[JsonIgnore]
	public int Length => 0;
}

public sealed record Col(int Color, [CallerArgumentExpression(nameof(Color))] string? Name = null);
