using LinqVec.Logic;
using PowMaybe;

namespace LinqVec.Structs;

public interface IId
{
	Guid Id { get; }
}



// - Can uniquely identify any entity in the model (model independent)
// - Can tell if the entity is in the model
public interface ISmartId : IEquatable<ISmartId>
{
	Guid Id { get; }
	bool Exists { get; }
}


// Can retrieve, set and delete the entity from the model
public interface ISmartId<E> : ISmartId
{
	E V { get; set; }
	void Delete();
}

public sealed class SmartId<M, E>(
	Guid id,
	IModelMan<M> mm,
	Func<M, Maybe<E>> find,
	Func<M, E, M> set,
	Action<M> delete
) : ISmartId<E>
{
	public Guid Id { get; } = id;

	public bool Exists => find(mm.V).IsSome();
	public E V
	{
		get => find(mm.V).Ensure();
		set => mm.V = set(mm.V, value);
	}
	public void Delete() => delete(mm.V);

	private bool Equals(SmartId<M, E> other) => other.Id == Id;
	public bool Equals(ISmartId? obj) => ReferenceEquals(this, obj) || obj is SmartId<M, E> other && Equals(other);
	public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is SmartId<M, E> other && Equals(other);
	public override int GetHashCode() => Id.GetHashCode();

	public static bool operator ==(SmartId<M, E>? left, SmartId<M, E>? right) => Equals(left, right);
	public static bool operator !=(SmartId<M, E>? left, SmartId<M, E>? right) => !Equals(left, right);

	public static bool operator ==(SmartId<M, E>? left, ISmartId<E>? right) => Equals(left, right);
	public static bool operator !=(SmartId<M, E>? left, ISmartId<E>? right) => !Equals(left, right);
	public static bool operator ==(SmartId<M, E>? left, ISmartId? right) => Equals(left, right);
	public static bool operator !=(SmartId<M, E>? left, ISmartId? right) => !Equals(left, right);

	public static bool operator ==(ISmartId<E>? left, SmartId<M, E>? right) => Equals(left, right);
	public static bool operator !=(ISmartId<E>? left, SmartId<M, E>? right) => !Equals(left, right);
	public static bool operator ==(ISmartId? left, SmartId<M, E>? right) => Equals(left, right);
	public static bool operator !=(ISmartId? left, SmartId<M, E>? right) => !Equals(left, right);
}
