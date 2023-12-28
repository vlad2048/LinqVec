using System.Reactive.Linq;
using System.Reactive.Subjects;
using LinqVec.Logic;
using LinqVec.Utils;
using LinqVec.Utils.Rx;
using ReactiveVars;

namespace LinqVec;



public interface IId
{
	Guid Id { get; }
}

public interface IDoc
{
	string GetUndoRedoStr();
}




/*  Responsibilities:
	=================
		- Holds the Doc
		- Provides Undo/Redo functionality
		- Holds a Ptr to do work on a single object in the Doc
		  - the Doc can only be modified through a Ptr
		  - the Ptr can hold a temporary Gfx change
		- Tells when to paint
		- Tells what to paint (includes the Ptr Gfx change)
	
	Usage:
	======
		IBoundVar<Doc> Cur                      official state of the Doc
		Doc            CurGfx                   state of the Doc to paint (including temporary Ptr Gfx change)
		IObservable<Unit> WhenPaintNeeded       when to repaint the Doc (can include mouse moves if the temporary Ptr Gfx change requires it)
		Undo()/Redo()                           undo/redo operations
		SetPtr(IPtr ptr)                        register a Ptr to do work on the Doc
												(unregisters the previous registered Ptr if any)

*/
/*
public interface IModel
{
	IObservable<Unit> WhenPaintNeeded { get; }
	IObservable<Unit> WhenUndoRedo { get; }
	void Undo();
	void Redo();
}

public sealed class Model<Doc> : IModel, IPtrMod<Doc> where Doc : IDoc
{
	private readonly Disp d;
	public void Dispose() => d.Dispose();

	private readonly Undoer<Doc> undoer;
	private readonly IRwVar<Option<IPtrDoc<Doc>>> ptr;

	public IBoundVar<Doc> Cur => undoer.Cur;

	public Model(Doc init, Disp d)
	{
		this.d = d;
		undoer = new Undoer<Doc>(init, d);
		ptr = VarUtils.MakeOptionalAutoDisp<IPtrDoc<Doc>>(d);
		mod = Option<Mod<Doc>>.None.MakeSafe(d);
		whenModEvt = new Subject<IModEvt>().D(d);

		G.Cfg.RunWhen(e => e.Log.UndoRedo, d, [() => undoer.PrintLog(doc => doc.GetUndoRedoStr())]);
	}


	// IModel
	// ======
	public IObservable<Unit> WhenPaintNeeded => Obs.Merge(
		undoer.Cur.ToUnit(),
		ptr.WhereSome().Select(e => e.WhenPaintNeeded).Switch(),
		ptr.ToUnit(),
		mod
			.Select(opt => opt.Match(
				m => m.Fun.ToUnit(),
				Obs.Never<Unit>
			))
			.Switch()
	);
	public IObservable<Unit> WhenUndoRedo => undoer.Cur.WhenInner.ToUnit();
	public void Undo() => undoer.Undo();
	public void Redo() => undoer.Redo();


	// IPtrMod<Doc>
	// ============
	private readonly IRwVar<Option<Mod<Doc>>> mod;
	private readonly Subject<IModEvt> whenModEvt;
	public IObservable<IModEvt> WhenModEvt => whenModEvt.AsObservable();
	public Doc V => Cur.V;
	public Doc ModGet() => mod.V.Match(m => m.Fun.V(V), () => V);
	public void ModSet(Mod<Doc> mod_)
	{
		ModFlush();
		if (!whenModEvt.IsDisposed) whenModEvt.OnNext(new SetModEvt(mod_.Name));
		mod.V = Some(mod_);
	}
	private void ModFlush()
	{
		mod.V.IfSome(m =>
		{
			if (m.ApplyWhenFinished)
			{
				if (!whenModEvt.IsDisposed) whenModEvt.OnNext(new ApplyModEvt(m.Name));
				Cur.V = m.Fun.V(V);
			}
			mod.V = None;
		});
	}

	public Doc GetGfx() =>
		mod.V.Match(
			m => m.Fun.V(GetGfxInner()),
			GetGfxInner
		);


	private Doc GetGfxInner() =>
		ptr.V.Match(
			p => p.Hide(),
			() => Cur.V
		);


	internal void PtrSet(IPtrDoc<Doc> ptr_)
	{
		if (ptr.V.IsSome) throw new ArgumentException("Ptr should be cleared before being reset");
		ptr.V = Some(ptr_);
	}
	internal void PtrClear(IPtrDoc<Doc> ptr_)
	{
		if (ptr.IsDisposed) return;
        if (ptr.V == Some(ptr_))
			ptr.V = None;
	}
}
*/