﻿using Geom;
using LinqVec.Structs;
using PowRxVar;

namespace LinqVec.Logic;

public static class Mod
{
    public static IModder<O> Mem<O>(O init, IRoVar<Option<Pt>> mousePos) where O : IId => new MemModder<O>(init, mousePos);
    public static IModder<O> Doc<O>(Lens<O> lens, IRoVar<Option<Pt>> mousePos) where O : IId => new DocModder<O>(lens, mousePos);
}

public interface IModder<O> : IUndoer where O : IId
{
	O V { get; }
	Func<O, Option<Pt>, O> Mod { get; set; }
	O VModded { get; }
	void Apply();
}

public sealed class MemModder<O> : IModder<O> where O : IId
{
    private static readonly Func<O, Option<Pt>, O> identity = (o, _) => o;
    private readonly Disp d = new();
    public void Dispose() => d.Dispose();

    private readonly IRoVar<Option<Pt>> mousePos;
    private readonly Undoer<O> undoer;

	// IModder
	// =======
    public O V
    {
        get => undoer.V;
        private set => undoer.V = value;
    }
    public Func<O, Option<Pt>, O> Mod { get; set; } = identity;
    public O VModded => Mod(V, mousePos.V);
    public void Apply()
    {
	    V = Mod(V, mousePos.V);
	    Mod = identity;
    }

	// IUndoer
	// =======
	public IObservable<Unit> WhenDo => undoer.WhenDo;
    public IObservable<Unit> WhenChanged => undoer.WhenChanged;
    public bool Undo() => undoer.Undo();
    public bool Redo() => undoer.Redo();
    public void InvalidateRedos() => undoer.InvalidateRedos();

    internal MemModder(O init, IRoVar<Option<Pt>> mousePos)
    {
	    undoer = new Undoer<O>(init).D(d);
        this.mousePos = mousePos;
    }
}


public sealed record Lens<O>(
    Func<O> Get,
    Action<O> Set,
    IObservable<Unit> WhenDisappear
) where O : IId;

public delegate Lens<O> LensGetter<D, O>(Model<D> doc, O obj) where D : IDoc where O : IId;


public sealed class DocModder<O> : IModder<O> where O : IId
{
	private static readonly Func<O, Option<Pt>, O> identity = (o, _) => o;
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly Lens<O> lens;
    private readonly IRoVar<Option<Pt>> mousePos;
	private readonly Undoer<O> undoer;

	public IObservable<Unit> WhenDisappear => lens.WhenDisappear;

	// IModder
	// =======
	public O V
	{
		get => lens.Get();
		private set
		{
			lens.Set(value);
			undoer.V = value;
		}
	}
	public Func<O, Option<Pt>, O> Mod { get; set; } = identity;
	public O VModded => Mod(V, mousePos.V);
	public void Apply()
	{
		V = Mod(V, mousePos.V);
		Mod = identity;
	}

	// IUndoer
	// =======
	public IObservable<Unit> WhenDo => undoer.WhenDo;
	public IObservable<Unit> WhenChanged => undoer.WhenChanged;
	public bool Undo() => undoer.Undo();
	public bool Redo() => undoer.Redo();
	public void InvalidateRedos() => undoer.InvalidateRedos();

	internal DocModder(Lens<O> lens, IRoVar<Option<Pt>> mousePos)
	{
        this.mousePos = mousePos;
        this.lens = lens;
	    undoer = new Undoer<O>(lens.Get()).D(d);
	}
}