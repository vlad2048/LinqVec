using PowMaybe;
using PowRxVar;

namespace LinqVec.Structs;


public interface IId { Guid Id { get; } }
public interface IVisualObj : IId
{
	R BoundingBox { get; }
	double DistanceToPoint(Pt pt);
}


public interface IDoc
{
	IId[] AllObjects { get; }
}


public interface IObj;
public interface IObj<O> : IObj where O : IId
{
	O V { get; set; }
}



/*public record TrkGet<D, O>(
	Func<D, O> Get,
	Func<D, O, D> Set,
	Func<D, D> Delete
) where D : IDoc where O : IId;

public sealed record TrkCreate<D, O>(
	Func<D, (D, O)> Create,
	Func<D, O> Get,
	Func<D, O, D> Set,
	Func<D, D> Delete
) : TrkGet<D, O>(Get, Set, Delete) where D : IDoc where O : IId;*/

//public interface IObjId;
//public sealed record EmptyId : IObjId;


/*
public sealed record EntityNfo<D, O>(
	Func<D, O, D> Add,
	Func<D, Guid, D> Del,
	Func<D, Guid, O> Get,
	Func<D, Guid, O, D> Set
)
	where D : IDoc
	where O : IId;
*/


/*
public interface IPtr
{
	Guid Id { get; }
}

public interface IPtr<O> : IPtr where O : IId
{
	O V { get; set; }
	Func<O, Maybe<Pt>, O> Mod { get; set; }
}



public static class PtrExt
{
	public static void ApplyMod<O>(this IPtr<O> ptr, Pt mousePt) where O : IId => ptr.V = ptr.Mod(ptr.V, May.Some(mousePt));
}
*/
