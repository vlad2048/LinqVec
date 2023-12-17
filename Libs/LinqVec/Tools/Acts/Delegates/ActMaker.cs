using LinqVec.Tools.Acts.Structs;

namespace LinqVec.Tools.Acts.Delegates;


public sealed record ActSet(
	string Name,
	Cursor Cursor,
	ActNfo[] Acts
)
{
	internal static readonly ActSet Empty = new("Empty", Cursors.Default, []);
}

public delegate ActSet ActMaker(Disp actD);




/*
public delegate Func<Disp, ActNfo[]> ActMaker(object hot);

public delegate Func<Disp, ActNfo[]> ActMaker<in H>(H hot);


static class ActMakerExt
{
	public static ActMaker? ToNonGeneric<H>(this ActMaker<H>? maker) => maker switch
	{
		null => null,
		not null => o => maker((H)o),
	};
}
*/