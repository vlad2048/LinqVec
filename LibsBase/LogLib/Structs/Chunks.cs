using System.Text.Json.Serialization;
using PowBasics.CollectionsExt;

namespace LogLib.Structs;

[JsonDerivedType(typeof(TextChunk), typeDiscriminator: "TextChunk")]
[JsonDerivedType(typeof(NewlineChunk), typeDiscriminator: "NewlineChunk")]
public interface IChunk
{
	[JsonIgnore]
	int Length { get; }
}

public sealed record TextChunk(
	string Text,
	Option<NamedColor> Fore,
	Option<NamedColor> Back
) : IChunk
{
	[JsonIgnore]
	public int Length => Text.Length;
}

public sealed record NewlineChunk : IChunk
{
	[JsonIgnore]
	public int Length => 0;
}




public static class ChunkExt
{
	public static IChunk[] SelectText(this IChunk[] chunks, Func<TextChunk, TextChunk> fun) => chunks.SelectToArray(chunk => chunk switch {
		TextChunk e => fun(e),
		_ => chunk
	});
	public static IChunk[] SetForeIfNull(this IChunk[] chunks, NamedColor fore) => chunks.SelectText(e => e with { Fore = e.Fore.IfNone(fore) });
	public static IChunk[] SetBack(this IChunk[] chunks, NamedColor back) => chunks.SelectText(e => e with { Back = e.Back.IfNone(back) });
}