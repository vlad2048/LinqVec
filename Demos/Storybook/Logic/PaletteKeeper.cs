using System.Reactive.Linq;
using System.Reactive.Subjects;
using LinqVec.Utils;
using LogLib.Structs;
using ReactiveVars;

namespace Storybook.Logic;

/*
	********************
	* Responsibilities *
	********************
		- Load the NamedColors from the CSharpFile
		- Sanity check the colors in Chunks are contained in it
		- Provide the DrawPanel with the actual color map Name->Color to draw the Chunks
		  so users can preview changes

*/

sealed class PaletteKeeper
{
	private readonly string csharpFile;
	private readonly ISubject<Unit> whenPaintNeeded;
	private readonly Dictionary<string, ColorNfo> map;
	private Option<ColorNfo> displayColorOverride = None;

	public IObservable<Unit> WhenPaintNeeded => whenPaintNeeded.AsObservable();

	public PaletteKeeper(string csharpFile, IChunk[] chunks, Disp d)
	{
		this.csharpFile = csharpFile;
		whenPaintNeeded = new Subject<Unit>().D(d);
		map = CSharpColorUtils.Load(csharpFile).ToDictionary(e => e.Name);
		SanityCheckChunksColorsAreInTheMap(map, chunks);
	}


	public void OverrideSet(string name, Color color)
	{
		displayColorOverride = map[name] with { Color = color };
		whenPaintNeeded.OnNext(Unit.Default);
	}
	public void OverrideReject()
	{
		displayColorOverride = None;
		whenPaintNeeded.OnNext(Unit.Default);
	}
	public void OverrideAccept()
	{
		var ovr = displayColorOverride.Ensure();
		map[ovr.Name] = ovr;
		displayColorOverride = None;
		whenPaintNeeded.OnNext(Unit.Default);
	}


	public void SaveChanges()
	{
		CSharpColorUtils.Save(csharpFile, map.Values.ToArray());
	}

	public Color GetColorForDisplay(string name)
	{
		if (displayColorOverride.Map(e => e.Name == name).IfNone(false))
			return displayColorOverride.Ensure().Color;
		return map[name].Color;
	}

	public Color GetColorForDisplayNoOverride(string name) => map[name].Color;



	private static void SanityCheckChunksColorsAreInTheMap(IReadOnlyDictionary<string, ColorNfo> map, IChunk[] chunks)
	{
		var chunkColors = chunks.OfType<TextChunk>().SelectMany(e => e.Back.Concat(e.Fore)).ToArray();
		foreach (var chunkColor in chunkColors)
			if (!map.ContainsKey(chunkColor.Name))
				throw new ArgumentException($"Missing color in S.cs: '{chunkColor.Name}'");
	}
}