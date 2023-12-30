using System.Reactive.Linq;
using ReactiveVars;

namespace PtrLib;


sealed class PtrDad<Dad> : PtrBase<Dad>, IPtr<Dad>
{
	private readonly IRwVar<Option<IPtrKid>> kid;

	public override Dad VModded
	{
		get
		{
			EnsureNotDisp();
			return base.VModded.RemoveKidEdit(kid);
		}
	}

	public PtrDad(Dad init, Disp d)
		: base(init, d)
	{
		kid = Option<IPtrKid>.None.Make(D);
	}

	private void SetKid(IPtrKid kidVal)
	{
		if (kid.V.IsSome) throw new ObjectDisposedException("Previous kid should have been disposed first");
		kid.V = Some(kidVal);
	}

	internal void Kid_Disposed(IPtrKid kidVal)
	{
		if (IsDisposed) return;		// it's OK to add this check here
		if (kid.V != Some(kidVal)) throw new ArgumentException("kid has been changed before it was disposed");
		kid.V = None;
	}

	internal void KidCreate_Commit<Kid, KidGizmo>(PtrKidCreate<Dad, Kid, KidGizmo> kidVal) where KidGizmo : IKidGizmo<Kid>
	{
		// Commit
		// ======
		if (kid.V != kidVal) throw new ArgumentException($"KidCreate<{typeof(Dad).Name}, {typeof(Kid).Name}> has been changed before it could be disposed");
		Undoer.ClearRedos();
		kidVal.Undoer.ClearRedos();
		foreach (var kidState in kidVal.Undoer.StackUndoExt)
			if (kidVal.ValidFun(kidState.V))
				V = kidVal.SetFun(V, kidState.V);

		// Dispose
		// =======
		kidVal.Dispose();
	}

	internal void KidEdit_Update<Kid, KidGizmo>(PtrKidEdit<Dad, Kid, KidGizmo> kidVal) where KidGizmo : IKidGizmo<Kid>
	{
		if (kid.V != kidVal) throw new ArgumentException($"KidEdit<{typeof(Dad).Name}, {typeof(Kid).Name}> has been changed before it could be disposed");
		V = kidVal.SetUpdatedValueInDad(V);
	}


	public IPtrRegular<KidGizmo> Edit<Kid, KidGizmo>(
		KidGizmo init,
		Func<Dad, Kid, Dad> setFun,
		Func<Dad, Kid, Dad> removeFun,
		Disp d
	) where KidGizmo : IKidGizmo<Kid>
	{
		var kidVal = new PtrKidEdit<Dad, Kid, KidGizmo>(
			init,
			setFun,
			removeFun,
			this,
			d
		);
		SetKid(kidVal);
		return kidVal;
	}

	public IPtrCommit<KidGizmo> Create<Kid, KidGizmo>(
		KidGizmo init,
		Func<Dad, Kid, Dad> setFun,
		Func<Kid, bool> validFun,
		Disp d
	) where KidGizmo : IKidGizmo<Kid>
	{
		var kidVal = new PtrKidCreate<Dad, Kid, KidGizmo>(
			init,
			setFun,
			validFun,
			this,
			d
		);
		SetKid(kidVal);
		return kidVal;
	}



	public IObservable<Unit> WhenPaintNeeded =>
		Obs.Merge([
			WhenPtrBasePaintNeeded,
			kid
				.Select(e => e.Match(
					f => f.WhenPtrBasePaintNeeded,
					() => Obs.Return(Unit.Default)
				))
				.Switch()
		]);

	public IObservable<Unit> WhenUndoRedo =>
		Obs.Merge([
			Undoer.Cur.WhenInner.ToUnit(),
			kid
				.Select(e => e.Match(
					f => f.WhenUndoRedo,
					() => Obs.Return(Unit.Default)
				))
				.Switch(),
		]);

	public void Undo() =>
		kid.V.Match(
			kidV =>
			{
				if (!kidV.Undo())
					Undoer.Undo();
			},
			() => Undoer.Undo()
		);

	public void Redo()
	{
		if (!Undoer.Redo())
			kid.V.IfSome(kidV => kidV.Redo());
	}

	public IObservable<Unit> WhenValueChanged => Undoer.Cur.ToUnit();
}



file static class DadExt
{
	public static Dad RemoveKidEdit<Dad>(this Dad v, IRoVar<Option<IPtrKid>> kid) =>
		kid.V.Match(
			kidVal => kidVal switch
				{
					IPtrKidEdit<Dad> kidEditVal => kidEditVal.RemoveFromDad(v),
					_ => v
				},
				() => v
			);
}