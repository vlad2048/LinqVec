using System.Reactive.Disposables;
using ReactiveVars;

namespace PtrLib;

interface IPtrKid : IHasDisp
{
	IObservable<Unit> WhenPtrBasePaintNeeded { get; }
	IObservable<Unit> WhenUndoRedo { get; }
	bool Undo();
	bool Redo();
}
interface IPtrKidCreate : IPtrKid;
interface IPtrKidEdit<Dad> : IPtrKid
{
	Dad RemoveFromDad(Dad v);
}


sealed class PtrKidEdit<Dad, Kid> : PtrBase<Kid>, IPtrKidEdit<Dad>, IPtrRegular<Kid>
{
	private readonly Func<Dad, Kid, Dad> setFun;
	private readonly Func<Dad, Kid, Dad> removeFun;

	public PtrKidEdit(
		Kid init,
		Func<Dad, Kid, Dad> setFun,
		Func<Dad, Kid, Dad> removeFun,
		PtrDad<Dad> dad,
		Disp d
	)
		: base(init, d)
	{
		this.setFun = setFun;
		this.removeFun = removeFun;
		Undoer.Cur.WhenOuter
			.Subscribe(_ =>
			{
				dad.KidEdit_Update(this);
			}).D(D);
		Disposable.Create(() =>
		{
			dad.Kid_Disposed(this);
		}).D(D);
	}

	public Dad SetUpdatedValueInDad(Dad v) => setFun(v, V);
	public Dad RemoveFromDad(Dad v) => removeFun(v, V);

	public IObservable<Unit> WhenUndoRedo => Undoer.Cur.WhenInner.ToUnit();
	public bool Undo() => false;
	public bool Redo() => false;
}


sealed class PtrKidCreate<Dad, Kid> : PtrBase<Kid>, IPtrKidCreate, IPtrCommit<Kid>
{
	private readonly PtrDad<Dad> dad;
	internal Func<Dad, Kid, Dad> SetFun { get; }
	internal Func<Kid, bool> ValidFun { get; }

	public PtrKidCreate(
		Kid init,
		Func<Dad, Kid, Dad> setFun,
		Func<Kid, bool> validFun,
		PtrDad<Dad> dad,
		Disp d
	)
		: base(init, d)
	{
		this.dad = dad;
		SetFun = setFun;
		ValidFun = validFun;

		Disposable.Create(() =>
		{
			dad.Kid_Disposed(this);
		}).D(D);
	}

	public void Commit()
	{
		dad.KidCreate_Commit(this);
		Dispose();
	}

	public IObservable<Unit> WhenUndoRedo => Undoer.Cur.WhenInner.ToUnit();
	public bool Undo() => Undoer.Undo();
	public bool Redo() => Undoer.Redo();
}
