using Geom;
using Newtonsoft.Json;

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
