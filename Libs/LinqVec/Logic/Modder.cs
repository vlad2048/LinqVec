using System.Reactive;
using LinqVec.Utils;
using PowMaybe;
using PowRxVar;

namespace LinqVec.Logic;

public static class Mod
{
    public static Modder<O> Make<O>(O init, IRoVar<Maybe<Pt>> mousePos) => new(init, mousePos);
}

public class Modder<O> : IUndoer
{
    private static readonly Func<O, Maybe<Pt>, O> identity = (o, _) => o;
    private readonly Disp d = new();
    public void Dispose() => d.Dispose();

    private readonly IRoVar<Maybe<Pt>> mousePos;
    private readonly Undoer<O> undoer;

    public O V
    {
        get => undoer.V;
        private set => undoer.V = value;
    }
    public Func<O, Maybe<Pt>, O> Mod { get; set; } = identity;
    public O VModded => Mod(V, mousePos.V);

    public IObservable<Unit> WhenDo => undoer.WhenDo;
    public IObservable<Unit> WhenChanged => undoer.WhenChanged;
    public bool Undo() => undoer.Undo();
    public bool Redo() => undoer.Redo();
    public void InvalidateRedos() => undoer.InvalidateRedos();

    internal Modder(O init, IRoVar<Maybe<Pt>> mousePos)
    {
	    undoer = new Undoer<O>(init, "Mod").D(d);
        this.mousePos = mousePos;
    }

    public void Apply()
    {
        V = Mod(V, mousePos.V);
        Mod = identity;
    }
}
