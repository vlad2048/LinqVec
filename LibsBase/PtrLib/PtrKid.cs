using System.Reactive.Disposables;
using ReactiveVars;

namespace PtrLib;

interface IPtrKid : IHasDisp;
interface IPtrKidCreate : IPtrKid;
interface IPtrKidEdit<Dad> : IPtrKid
{
	Dad RemoveFromDad(Dad v);
}

public sealed class PtrKidCreate<Dad, Kid> : PtrBase<Kid>, IPtrKidCreate
{
	private readonly PtrDad<Dad> dad;
	internal Func<Dad, Kid, Dad> SetFun { get; }
	internal Func<Kid, bool> ValidFun { get; }

	public PtrKidCreate(
		Kid init,
		Func<Dad, Kid, Dad> setFun,
		Func<Kid, bool> validFun,
		PtrDad<Dad> dad
	)
		: base(init, MkD())
	{
		this.dad = dad;
		SetFun = setFun;
		ValidFun = validFun;

		Disposable.Create(() =>
		{
			dad.Kid_Disposed(this);
		}).D(D);
	}

	public void CommitDispose()
	{
		dad.KidCreate_Commit(this);
		//Dispose();	(done by the dad)
	}
}


public sealed class PtrKidEdit<Dad, Kid> : PtrBase<Kid>, IPtrKidEdit<Dad>
{
	private readonly Func<Dad, Kid, Dad> setFun;
	private readonly Func<Dad, Kid, Dad> removeFun;

	public PtrKidEdit(
		Kid init,
		Func<Dad, Kid, Dad> setFun,
		Func<Dad, Kid, Dad> removeFun,
		PtrDad<Dad> dad
	)
		: base(init, MkD())
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
}