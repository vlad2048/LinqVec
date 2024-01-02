using LogLib.Interfaces;
using LogLib.Structs;
using PowBasics.Json_;
using System.Text.Json.Serialization;
using PowBasics.CollectionsExt;
using ArgumentException = System.ArgumentException;

namespace LogLib.Writers;

[JsonDerivedType(typeof(TextChunk), typeDiscriminator: "TextChunk")]
[JsonDerivedType(typeof(NewlineChunk), typeDiscriminator: "NewlineChunk")]
public interface IChunk;
public sealed record TextChunk(TxtSegment Seg) : IChunk;
public sealed record NewlineChunk : IChunk;

public sealed record GenNfo<TIWriteSer>(TIWriteSer Src, IChunk[] Chunks) where TIWriteSer : IWrite;



file static class ConWriterEnumFuncExt
{
	public static Func<T, T> ApplyAll<T>(this IEnumerable<Func<T, T>> funs) => w => funs.Aggregate(w, (w_, f) => f(w_));

	public static Func<Option<T>> EnableWhen<T>(this Func<T> funPrev, Func<bool> enable) =>
		() => enable() switch {
			false => None,
			true => Some(funPrev())
		};
}




public sealed class ConWriter<TIWriteSer> where TIWriteSer : IWrite
{
	public static readonly ConWriter<TIWriteSer> Instance = new();

	// *********
	// * State *
	// *********
	private readonly List<IPrefix> prefixes = new();
	private readonly List<GenNfo<TIWriteSer>> gens = new();
	private GenNfo<TIWriteSer>[] Gens => gens.ToArray();
	public void Reset()
	{
		prefixes.Clear();
		gens.Clear();
	}


	private ConWriter() {}


	// ************
	// * Prefixes *
	// ************
	private interface IPrefix
	{
		string Name { get; }
		ITxtWriter Write(ITxtWriter writer);
	}
	private sealed record Prefix<T>(string Name, Func<Option<T>> valFun) : IPrefix where T : IWrite
	{
		public ITxtWriter Write(ITxtWriter writer) => valFun().Match(val => val.Write(writer), () => writer);
	}
	private sealed class DisposeAction(Action action) : IDisposable { public void Dispose() => action(); }
	private void AddPrefix<T>(string name, Func<Option<T>> valFun) where T : IWrite => prefixes.Add(new Prefix<T>(name, valFun));
	private void RemovePrefix(string name) { var toDels = prefixes.WhereToArray(e => e.Name == name); foreach (var toDel in toDels) prefixes.Remove(toDel); }

	public IDisposable RegisterPrefix<T>(string name, Func<Option<T>> valFun) where T : IWrite
	{
		AddPrefix(name, valFun);
		return new DisposeAction(() => RemovePrefix(name));
	}

	
	public IDisposable RegisterPrefix<T>(string name, Func<T> valFun, Func<bool> enabledFun) where T : TIWriteSer
	{
		AddPrefix(name, valFun.EnableWhen(enabledFun));
		return new DisposeAction(() => RemovePrefix(name));
	}





	// **********
	// * Chunks *
	// **********
	private sealed class ChunkyTxtWriter : ITxtWriter
	{
		private readonly List<IChunk> chunks = new();
		private readonly Func<ITxtWriter, ITxtWriter> prefixesFun;
		private bool isNewLine = true;
		public IChunk[] Chunks => chunks.ToArray();
		public int LastSegLength => chunks.Count switch
		{
			0 => 0,
			_ => chunks.Last() switch
			{
				TextChunk { Seg.Text: var text } => text.Length,
				_ => 0
			}
		};

		public ChunkyTxtWriter(Func<ITxtWriter, ITxtWriter> prefixesFun)
		{
			this.prefixesFun = prefixesFun;
		}

		public ITxtWriter Write(TxtSegment seg)
		{
			if (isNewLine)
			{
				isNewLine = false;
				prefixesFun(this);
			}
			chunks.Add(new TextChunk(seg));
			return this;
		}

		public ITxtWriter WriteLine()
		{
			chunks.Add(new NewlineChunk());
			isNewLine = true;
			return this;
		}
	}

	public ConWriter<TIWriteSer> Gen(TIWriteSer src)
	{
		static Func<ITxtWriter, ITxtWriter> Mk(Func<ITxtWriter, ITxtWriter> f) => f;

		var chunkyWriter = new ChunkyTxtWriter(prefixes.Select(e => Mk(e.Write)).ApplyAll());
		src.Write(chunkyWriter);
		chunkyWriter.WriteLine();
		var chunks = chunkyWriter.Chunks;
		gens.Add(new GenNfo<TIWriteSer>(src, chunks));
		foreach (var chunk in chunks)
		{
			switch (chunk)
			{
				case TextChunk { Seg: { Text: var text, Color: var color } }:
					L.Write(text, color);
					break;
				case NewlineChunk:
					L.WriteLine();
					break;
				default:
					throw new ArgumentException();
			}
		}
		return this;
	}


	// *****************
	// * Serialization *
	// *****************
	public void Save(Jsoner jsoner, string filename)
	{
		jsoner.Save(filename, Gens);
	}



	public void Replay(Jsoner jsoner, string file)
	{
		Reset();

		var gens_ = jsoner.Load<GenNfo<TIWriteSer>[]>(file);
		foreach (var gen in gens_)
		{
			var (src, chunks) = gen;
			Gen(src);
		}
	}
}