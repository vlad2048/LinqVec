using System.Reactive;
using LinqVec.Structs;
using PowRxVar;

namespace LinqVec.Logic;


public sealed class Model<D> : IUndoer, IDisposable where D : IDoc
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly Undoer<D> undoer;

	public D V
	{
		get => undoer.V;
		set => undoer.V = value;
	}

	public IObservable<Unit> WhenDo => undoer.WhenDo;
	public IObservable<Unit> WhenChanged => undoer.WhenChanged;
	public bool Undo() => undoer.Undo();
	public bool Redo() => undoer.Redo();
	public void InvalidateRedos() => undoer.InvalidateRedos();

	public Model(D init)
	{
		undoer = new Undoer<D>(init, "Doc").D(d);
	}


	/*
	public (IPtrSlot<O>, IDisposable) RegisterSlot<O>() where O : IId => slotter.Register<O>();

	public IPtr<O> Create<O>(TrkCreate<D, O> trk) where O : IId
	{
		(doc.V, var obj) = trk.Create(doc.V);
		var ptr = new Ptr<O>(doc, trk);
		AddPtr(ptr);
		return ptr;
	}

	public IPtr<O> Get<O>(TrkGet<D, O> trk) where O : IId
	{
		var ptr = new Ptr<O>(doc, trk);
		AddPtr(ptr);
		return ptr;
	}


	private void AddPtr<O>(IPtr<O> ptr) where O : IId
	{
		ptrsSrc.AddOrUpdate(ptr);
		ptr.WhenDisposed.Subscribe(_ => ptrsSrc.Remove(ptr.Id));
	}

	private sealed class Ptr<O>(
		IRwVar<D> doc,
		TrkGet<D, O> trk
	) : IPtr<O> where O : IId
	{
		private readonly Disp d = new();
		public void Dispose() => d.Dispose();
		public bool IsDisposed => d.IsDisposed;
		public IObservable<Unit> WhenDisposed => d.WhenDisposed;

		public Guid Id { get; } = trk.Get(doc.V).Id;
		public O V
		{
			get => trk.Get(doc.V);
			set => doc.V = trk.Set(doc.V, value);
		}

		public Func<O, Maybe<Pt>, O> Mod { get; set; } = (o, _) => o;
	}
	*/
}



/*
public interface IModelMan<M>
{
	M V { get; set; }
}


public class ModelMan<M> : IModelMan<M>, IDisposable
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly Undoer<M> model;
	private readonly ISourceCache<IEntityM<M>, Guid> trackedSrc;
	private readonly IObservable<IChangeSet<IEntityM<M>, Guid>> tracked;

	public M V
	{
		get => model.V;
		set => model.Do(value);
	}

	public M GetGfxModel(IRoMayVar<Pt> mp) => V;
		//trackedSrc.Items
		//	.Aggregate(
		//		V,
		//		(m, e) => e.GfxCommit(m, mp)
		//	);

	public IObservable<Unit> WhenChanged =>
		Obs.Merge(
			model.WhenChanged,
			tracked.ToUnit()
		);

	public IId[] GetTracked() => trackedSrc.Items.SelectToArray(e => e.GetV());
		
	public IObservable<Unit> WhenUndoRedo => model.WhenUndoRedo;

	public ModelMan(
		M init,
		IObservable<IEvt> whenEvt
	)
	{
		model = new Undoer<M>(init, whenEvt).D(d);
		trackedSrc = new SourceCache<IEntityM<M>, Guid>(e => e.GetV().Id).D(d);
		tracked = trackedSrc.Connect();
	}

	public IEntity<E> Create<E>(Func<ModelMan<M>, IEntity<M, E>> make) where E : IId
	{
		var entity = make(this);
		trackedSrc.AddOrUpdate(entity);
		entity.WhenChanged.Subscribe(_ => trackedSrc.AddOrUpdate(entity)).D(entity);
		entity.WhenDisposed.Subscribe(_ =>
		{
			trackedSrc.Remove(entity.V.Id);
		});
		return entity;
	}

	public IEntity<E> Get<E>(E e) where E : IId
	{

	}

	public bool IsTracked<E>(E entity) where E : IId => trackedSrc.Items.Any(e => e.GetV().Id == entity.Id);
}
*/