using ReactiveVars;

namespace PtrLib;

public sealed class PtrDad<Dad> : PtrBase<Dad>
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

	internal void KidCreate_Commit<Kid>(PtrKidCreate<Dad, Kid> kidVal)
	{
		// Commit
		// ======
		if (kid.V != kidVal) throw new ArgumentException($"KidCreate<{typeof(Dad).Name}, {typeof(Kid).Name}> has been changed before it could be disposed");
		Undoer.ClearRedos();
		kidVal.Undoer.ClearRedos();
		foreach (var kidState in kidVal.Undoer.StackUndoExt)
			if (kidVal.ValidFun(kidState))
				V = kidVal.SetFun(V, kidState);

		// Dispose
		// =======
		kidVal.Dispose();
		kid.V = None;
	}

	internal void KidEdit_Update<Kid>(PtrKidEdit<Dad, Kid> kidVal)
	{
		if (kid.V != kidVal) throw new ArgumentException($"KidEdit<{typeof(Dad).Name}, {typeof(Kid).Name}> has been changed before it could be disposed");
		V = kidVal.SetUpdatedValueInDad(V);
	}


	public PtrKidCreate<Dad, Kid> Create<Kid>(
		Kid init,
		Func<Dad, Kid, Dad> setFun,
		Func<Kid, bool> validFun
	)
	{
		var kidVal = new PtrKidCreate<Dad, Kid>(
			init,
			setFun,
			validFun,
			this
		);
		SetKid(kidVal);
		return kidVal;
	}

	public PtrKidEdit<Dad, Kid> Edit<Kid>(
		Kid init,
		Func<Dad, Kid, Dad> setFun,
		Func<Dad, Kid, Dad> removeFun
	)
	{
		var kidVal = new PtrKidEdit<Dad, Kid>(
			init,
			setFun,
			removeFun,
			this
		);
		SetKid(kidVal);
		return kidVal;
	}
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