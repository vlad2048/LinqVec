using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using LinqVec.Structs;
using LinqVec.Tools.Events;
using PowMaybe;
using PowRxVar;
using LinqVec.Utils;

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
	private readonly IRwVar<IEntityM<M>[]> tracked;

	public M V
	{
		get => model.V;
		set => model.Do(value);
	}

	public M GetGfxModel(IRoMayVar<Pt> mp) =>
		tracked.V
			.Aggregate(
				V,
				(m, e) => e.GfxCommit(m, mp)
			);

	public IObservable<Unit> WhenChanged =>
		Obs.Merge(
			model.WhenChanged,
			tracked
				.Select(ts =>
					Obs.Merge(
						ts.Select(t => t.WhenChanged).Merge(),
						ts.Select(t => t.WhenInvalidated).Merge()
					)
				)
				.Switch()
		);
		
		//model.WhenChanged;
	public IObservable<Unit> WhenUndoRedo => model.WhenUndoRedo;

	public ModelMan(
		M init,
		IObservable<IEvtGen<PtInt>> whenEvt
	)
	{
		model = new Undoer<M>(init, whenEvt).D(d);
		tracked = Var.Make(Array.Empty<IEntityM<M>>()).D(d);

		/*model.WhenChanged
			.Where(_ => entityEdited.V.IsSome(out var entityEdit_) && !entityEdit_.Exists)
			.Subscribe(_ =>
			{
				entityEdited.V = May.None<ISmartId>();
				requireToolReset();
			}).D(d);*/

	}

	public IEntity<E> Create<E>(Func<ModelMan<M>, IEntity<M, E>> make)
	{
		var entity = make(this);
		tracked.V = tracked.V.Add(entity.D(d));
		entity.WhenInvalidated.Subscribe(_ =>
		{
			entity.Dispose();
			tracked.V = tracked.V.Remove(entity);
		});
		return entity;
	}
}












/*
public class ModelMan<M> : IModelMan<M>, IDisposable
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly Undoer<M> model;
	private readonly IRwMayVar<ISmartId> entityEdited;

	public M V
	{
		get => model.V;
		set => model.Do(value);
	}

	public IObservable<Unit> WhenChanged => model.WhenChanged;
	public IObservable<Unit> WhenUndoRedo => model.WhenUndoRedo;

	public ModelMan(
		M init,
		IObservable<IEvtGen<PtInt>> whenEvt,
		Action requireToolReset
	)
	{
		model = new Undoer<M>(init, whenEvt).D(d);
		entityEdited = VarMay.Make<ISmartId>().D(d);

		model.WhenChanged
			.Where(_ => entityEdited.V.IsSome(out var entityEdit_) && !entityEdit_.Exists)
			.Subscribe(_ =>
			{
				entityEdited.V = May.None<ISmartId>();
				requireToolReset();
			}).D(d);

	}

	public bool IsEdited<E>(E entity) where E : IId => entityEdited.V.IsSome(out var edited) && edited.Id == entity.Id;

	public ISmartId<E> Create<E>(Func<ModelMan<M>, Func<M, (M, ISmartId<E>)>> make)
	{
		var (mNext, id) = make(this)(V);
		V = mNext;
		return id;
	}

	public (ISmartId<E>, IDisposable) CreateEdit<E>(Func<ModelMan<M>, Func<M, (M, ISmartId<E>)>> make)
	{
		var id = Create(make);
		var toolD = new Disp();
		entityEdited.V = May.Some((ISmartId)id);
		toolD.WhenDisposed.Subscribe(_ => entityEdited.V = May.None<ISmartId>());
		return (id, toolD);
	}
}
*/
