using Geom;
using ReactiveVars;

namespace LinqVec.Logic;


/*
public delegate O MouseMod<O>(O obj, Pt mousePos);


public interface IMouseModder<O>
{
	IUndoer Undoer { get; }
	O Get();
	void ModSet(MouseMod<O> modFun);
	void ModClear();
	void ModApply(Pt mousePos);
}
*/



//public delegate MouseMod<O> MouseModStart<O>(Pt startPos);

//public delegate MouseMod<O> MouseModStartHot<O, in H>(Pt startPos, H hot);

/*
public interface IMouseModder<O>
{
	IUndoer Undoer { get; }
	O Get();
	void ModSet(MouseMod<O> modFun);
	void ModClear();
	void ModApply(Pt mousePos);
}


public sealed class MemMouseModder<O> : IDisposable, IMouseModder<O>
{
	private static readonly MouseMod<O> identity = (o, _) => o;
	private readonly Disp d = MkD();
	public void Dispose() => d.Dispose();

    private readonly Undoer<O> undoer;
    private MouseMod<O> mod = identity;

	public MemMouseModder(O init)
    {
	    undoer = new Undoer<O>(init).D(d);
    }

	public O GetModded(Option<Pt> mousePos) => mousePos.Map(m => mod(undoer.V, m)).IfNone(undoer.V);

	// IMouseModder
	// ------------
	public IUndoer Undoer => undoer;
	public O Get() => undoer.V;
	public void ModSet(MouseMod<O> modFun) => mod = modFun;
	public void ModClear() => mod = identity;
	public void ModApply(Pt mousePos) { undoer.V = GetModded(mousePos); mod = identity; }
}


public sealed class DocMouseModder<O> : IDisposable, IMouseModder<O>
{
	private static readonly MouseMod<O> identity = (o, _) => o;
	private readonly Disp d = MkD();
	public void Dispose() => d.Dispose();

	private readonly Func<O> getFun;
	private readonly Action<O> setFun;
	private MouseMod<O> mod = identity;

	public DocMouseModder(Func<O> getFun, Action<O> setFun)
	{
		this.getFun = getFun;
		this.setFun = setFun;
	}

	public O GetModded(Option<Pt> mousePos) => mousePos.Map(m => mod(getFun(), m)).IfNone(getFun());
	
	public IUndoer Undoer => Logic.Undoer.Empty;
	public O Get() => getFun();
	public void ModSet(MouseMod<O> modFun) => mod = modFun;
	public void ModClear() => mod = identity;
	public void ModApply(Pt mousePos)
	{
		setFun(GetModded(mousePos));
		mod = identity;
	}
}
*/
