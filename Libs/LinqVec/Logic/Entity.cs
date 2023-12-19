using System.Reactive.Subjects;
using System.Reactive;
using System.Reactive.Linq;
using LinqVec.Structs;

namespace LinqVec.Logic;


/*
public sealed record EntityNfo<M, E>(
	Func<M, (M, FullId)> Make,
	Func<M, FullId, E, M> Change,
	Func<M, FullId, M> Delete,
	Func<M, FullId, Maybe<E>> Find
);

public interface IEntity : IDisposable
{
	bool CanFind { get; }
	bool IsThisOne(object obj);
}

public sealed class Entity<M, E> : IEntity, IDisposable
{
	private readonly Disp d = MkD();
	public void Dispose() => d.Dispose();

	private readonly EntityNfo<M, E> entityNfo;
	private readonly FullId fullId;
	private readonly ModelMan<M> modelMan;
	private bool isDone;
	private readonly ISubject<Unit> whenRemoved;
	public IObservable<Unit> WhenRemoved => whenRemoved.AsObservable();

	public Entity(EntityNfo<M, E> entityNfo, FullId fullId, ModelMan<M> modelMan)
	{
		this.entityNfo = entityNfo;
		this.fullId = fullId;
		this.modelMan = modelMan;
		whenRemoved = new AsyncSubject<Unit>().D(d);
	}

	private void Stop()
	{
		if (isDone) throw new ArgumentException();
		isDone = true;
		whenRemoved.OnNext(Unit.Default);
		whenRemoved.OnCompleted();
	}

	public bool CanFind => entityNfo.Find(modelMan.V, fullId).IsSome();
	public bool IsThisOne(object obj) => entityNfo.Find(modelMan.V, fullId).IsSome(out var findElt) && findElt.Equals(obj);

	public E V
	{
		get
		{
			if (isDone) throw new ArgumentException();
			return entityNfo.Find(modelMan.V, fullId).Ensure();
		}
		set
		{
			if (isDone) throw new ArgumentException();
			modelMan.Do(entityNfo.Change(modelMan.V, fullId, value));
		}
	}

	public void Delete()
	{
		Stop();
		var mNext = entityNfo.Delete(modelMan.V, fullId);
		modelMan.Do(mNext);
	}

	public void Commit()
	{
		Stop();
	}
}
*/