using System.Reactive;
using DynamicData;
using LinqVec.Structs;
using LinqVec.Tools.Events;
using PowRxVar;
using PowBasics.CollectionsExt;

namespace LinqVec.Logic;

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

	public M GetGfxModel(IRoMayVar<Pt> mp) =>
		trackedSrc.Items
			.Aggregate(
				V,
				(m, e) => e.GfxCommit(m, mp)
			);

	public IObservable<Unit> WhenChanged =>
		Obs.Merge(
			model.WhenChanged,
			tracked.ToUnit()
		);

	public IId[] GetTracked() => trackedSrc.Items.SelectToArray(e => e.GetV());
		
	public IObservable<Unit> WhenUndoRedo => model.WhenUndoRedo;

	public ModelMan(
		M init,
		IObservable<IEvtGen<PtInt>> whenEvt
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

	public bool IsTracked<E>(E entity) where E : IId => trackedSrc.Items.Any(e => e.GetV().Id == entity.Id);
}
