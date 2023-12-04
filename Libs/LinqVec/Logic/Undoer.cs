﻿using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using LinqVec.Tools.Events;
using LinqVec.Utils.WinForms_;
using PowRxVar;

namespace LinqVec.Logic;

public sealed class Undoer<T> : IDisposable
{
    private readonly Disp d = new();
    public void Dispose() => d.Dispose();

    private readonly IRwVar<T> cur;
    private readonly Stack<T> stackUndo = new();
    private readonly Stack<T> stackRedo = new();
    private readonly ISubject<T> whenDo;
    private readonly ISubject<Unit> whenUndo;
    private readonly ISubject<Unit> whenRedo;
    private IObservable<T> WhenDo => whenDo.AsObservable();
    private IObservable<Unit> WhenUndo => whenUndo.AsObservable();
    private IObservable<Unit> WhenRedo => whenRedo.AsObservable();
    private void Undo() => whenUndo.OnNext(Unit.Default);
    private void Redo() => whenRedo.OnNext(Unit.Default);

    public T V => cur.V;
    public IObservable<Unit> WhenChanged => cur.ToUnit();
    public void Do(T val) => whenDo.OnNext(val);
    public IObservable<Unit> WhenUndoRedo => Obs.Merge(WhenUndo, WhenRedo);


    public Undoer(T init, IObservable<IEvtGen<PtInt>> whenEvt)
    {
        cur = Var.Make(init).D(d);
        stackUndo.Push(init);
        whenDo = new Subject<T>().D(d);
        whenUndo = new Subject<Unit>().D(d);
        whenRedo = new Subject<Unit>().D(d);

        WhenDo.Subscribe(v =>
        {
            stackUndo.Push(v);
            stackRedo.Clear();
            cur.V = v;
        }).D(d);

        WhenUndo.Subscribe(_ =>
        {
            if (stackUndo.Count < 2) return;
            var valUndo = stackUndo.Pop();
            stackRedo.Push(valUndo);
            cur.V = stackUndo.Peek();
        }).D(d);

        WhenRedo.Subscribe(_ =>
        {
            if (!stackRedo.TryPop(out var valRedo))
                return;
            stackUndo.Push(valRedo);
            cur.V = valRedo;
        }).D(d);


        whenEvt.WhenKeyRepeat(Keys.Z, true).Subscribe(_ => Undo()).D(d);
        whenEvt.WhenKeyRepeat(Keys.Y, true).Subscribe(_ => Redo()).D(d);
    }
}