using LinqVec;
using VectorEditor._Model.Interfaces;

namespace VectorEditor._Model;

public sealed record Layer(
	Guid Id,
	IObj[] Objects
) : IId
{
	public static Layer Empty() => new(Guid.NewGuid(), []);
}