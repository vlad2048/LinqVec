using Geom;
using PowRxVar;

namespace LinqVec.Logic;


public delegate O MouseMod<O>(O obj, Pt mousePos);

public interface IMouseModder<O>
{
	IUndoer Undoer { get; }
	O Get();
	void ModSet(MouseMod<O> modFun);
	void ModClear();
	void ModApply(Pt mousePos);
}


public sealed class MouseModder<O> : IMouseModder<O>, IDisposable
{
	private static readonly MouseMod<O> identity = (o, _) => o;
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

    private readonly Undoer<O> undoer;
    private MouseMod<O> mod = identity;

	public MouseModder(O init)
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
