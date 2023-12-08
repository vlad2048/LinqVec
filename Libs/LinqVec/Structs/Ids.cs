using System.Reactive;
using LinqVec.Logic;
using LinqVec.Utils.Rx;
using PowMaybe;
using PowRxVar;
using PowRxVar.Utils;

namespace LinqVec.Structs;

public interface IId
{
	Guid Id { get; }
}



public enum EntityState
{
	Uncommited,
	Commited,
	Invalid
}

public interface IEntity
{
	IObservable<Unit> WhenChanged { get; }
	IObservable<Unit> WhenInvalidated { get; }
	EntityState State { get; }
	bool IsValid();
	void Commit();
	void Delete();
	void Invalidate();
}

public interface IEntityM<M> : IEntity
{
	M GfxCommit(M m, IRoMayVar<Pt> mousePos);
}

public interface IEntity<E> : IEntity
{
	E V { get; set; }
	void ModSet(Func<E, Maybe<Pt>, E> mod);
	void ModApply(Maybe<Pt> mousePos);
}

public interface IEntity<M, E> : IEntityM<M>, IEntity<E>, IDisposable
{
}

public sealed class Entity<M, E> : IEntity<M, E>
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly IModelMan<M> mm;
	private readonly Func<M, bool, bool> isValid;
	private readonly Func<M, E, M> add;
	private readonly Func<M, M> delete;
	private readonly Func<M, E> get;
	private readonly Func<M, E, M> set;
	private readonly Action<Unit> sigInvalidate;
	private readonly Action<Unit> sigChanged;

	private EntityState state;
	private Maybe<E> uncommitedV;

	public IObservable<Unit> WhenInvalidated { get; }
	public IObservable<Unit> WhenChanged { get; }
	private Func<E, Maybe<Pt>, E> mod = (e, _) => e;

	public Entity(
		IModelMan<M> mm,
		E init,
		Func<M, bool, bool> isValid,
		Func<M, E, M> add,
		Func<M, M> delete,
		Func<M, E> get,
		Func<M, E, M> set
	)
	{
		this.mm = mm;
		this.isValid = isValid;
		this.add = add;
		this.delete = delete;
		this.get = get;
		this.set = set;
		uncommitedV = May.Some(init);
		(sigInvalidate, WhenInvalidated) = Sig.Make<Unit>();
		(sigChanged, WhenChanged) = RxEventMaker.Make<Unit>().D(d);
	}

	public void ModSet(Func<E, Maybe<Pt>, E> mod_)
	{
		mod = mod_;
		sigChanged(Unit.Default);
	}

	public void ModApply(Maybe<Pt> mousePos)
	{
		V = mod(V, mousePos);
		mod = (e, _) => e;
	}

	public M GfxCommit(M m, IRoMayVar<Pt> mousePos) => State switch
	{
		EntityState.Uncommited => add(m, mod(V, mousePos.V)),
		EntityState.Commited => set(m, mod(V, mousePos.V)),
		EntityState.Invalid => throw new ArgumentException("Invalid entity"),
		_ => throw new ArgumentException()
	};

	public EntityState State
	{
		get => state;
		private set
		{
			if (value == state) return;
			state = value;
			if (state == EntityState.Invalid)
				sigInvalidate(Unit.Default);
		}
	}

	public E V
	{
		get => State switch
		{
			EntityState.Uncommited => uncommitedV.Ensure(),
			EntityState.Commited => get(mm.V),
			EntityState.Invalid => throw new ArgumentException("Invalid entity"),
			_ => throw new ArgumentException()
		};
		set
		{
			switch (State)
			{
				case EntityState.Uncommited:
					if (uncommitedV.IsNone()) throw new ArgumentException();
					uncommitedV = May.Some(value);
					break;
				case EntityState.Commited:
					if (uncommitedV.IsSome()) throw new ArgumentException();
					mm.V = set(mm.V, value);
					break;
				case EntityState.Invalid:
					throw new ArgumentException("Invalid entity");
				default:
					throw new ArgumentException();
			}
			sigChanged(Unit.Default);
		}
	}

	public bool IsValid()
	{
		if (State == EntityState.Invalid) throw new ArgumentException("Invalid entity");
		return isValid(mm.V, State == EntityState.Commited);
	}

	public void Commit()
	{
		if (State != EntityState.Uncommited) throw new ArgumentException($"Cannot Commit() when State={State}");
		if (uncommitedV.IsNone(out var val)) throw new ArgumentException();
		mm.V = add(mm.V, val);
		uncommitedV = May.None<E>();
	}

	public void Delete()
	{
		switch (State)
		{
			case EntityState.Uncommited:
				break;
			case EntityState.Commited:
				mm.V = delete(mm.V);
				break;
			case EntityState.Invalid:
				throw new ArgumentException("Invalid entity");
			default:
				throw new ArgumentException();
		}
		Invalidate();
	}

	public void Invalidate()
	{
		if (State == EntityState.Invalid) throw new ArgumentException("Cannot Invalidate() an Invalid entity");
		State = EntityState.Invalid;
	}
}

public static class EntityExt
{
	public static void ModApply<E>(this IEntity<E> entity, IRoMayVar<Pt> mousePos) =>
		entity.ModApply(mousePos.V);

	public static void ModApply<E>(this IEntity<E> entity, Pt mousePos) =>
		entity.ModApply(May.Some(mousePos));
}




/*
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
*/