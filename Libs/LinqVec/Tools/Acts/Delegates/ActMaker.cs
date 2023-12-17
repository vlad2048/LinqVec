using LinqVec.Tools.Acts.Structs;

namespace LinqVec.Tools.Acts.Delegates;

public delegate ActNfo[] ActMaker(object hot);

public delegate ActNfo[] ActMaker<in H>(H hot);


static class ActMakerExt
{
	public static ActMaker? ToNonGeneric<H>(this ActMaker<H>? maker) => maker switch
	{
		null => null,
		not null => o => maker((H)o),
	};
}