<Query Kind="Program" />

#load "_common\rx"
// System
using System.Threading.Tasks;

// Reactive
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Disposables;
using System.Reactive.Concurrency;
using Microsoft.Reactive.Testing;
using Obs = System.Reactive.Linq.Observable;
using Disp = System.Reactive.Disposables.CompositeDisposable;

// LanguageExt
using Unit = LanguageExt.Unit;

// ReactiveVars
using ReactiveVars;
using static ReactiveVars.DispMaker;

// LINQPad
using static RxTestMakers;



void Main()
{
	var doc = new Model<Doc>(Doc.Empty()).D(D);
}


public sealed class Ptr<D, O> : IDisposable
{
	private readonly Disp d = MkD();
	public void Dispose() => d.Dispose();
	
	private readonly Model<D> doc;
	private readonly Lens<D, O> lens;

	private Ptr(Model<D> doc, Lens<D, O> lens)
	{
		this.doc = doc;
		this.lens = lens;
	}
}


/*
public sealed record Lens<D, O>(
	Func<D, O, D> Add,
	Func<D, D> Del,
	Func<D, bool> IsValid,
	Func<D, O> Get,
	Func<D, O, D> Set
);
*/


public sealed class CurvePtr : IDisposable
{
	private readonly Disp d = MkD();
	public void Dispose() => d.Dispose();

	private readonly Model<Doc> doc;
	private readonly Undoer<Curve> undoer;
	private readonly Guid layerId;
	private readonly Guid curveId;
	
	public Curve V => (Curve)doc.V.Layers.Get(layerId).Objects.Get(curveId);
	
	private CurvePtr(Model<Doc> doc, Guid? create, (Guid, Guid)? edit)
	{
		this.doc = doc;
		if (create.HasValue)
		{
			var curve = Curve.Empty();
			(layerId, curveId) = (doc.V.Layers[0].Id, curve.Id);
			doc.V = doc.V.ChangeLayer(layerId, objs => objs.Add(curve));
		}
		else
		{
			(layerId, curveId) = (edit!.Value.Item1, edit.Value.Item2);
		}
		
		undoer = new Undoer<Curve>(V).D(d);
	}
	
	public static CurvePtr Create(Model<Doc> doc, Guid layerId) => new(doc, layerId, null);
	public static CurvePtr Edit(Model<Doc> doc, Guid layerId, Guid curveId) => new(doc, null, (layerId, curveId));
}


/*
public static class Lenses
{
	public static Lens<Doc, Curve> Curve(Guid layerId, Guid curveId) =>
		new Lens<Doc, Curve>(
			(doc, val) =>
			{
				if (val.Id != curveId) throw new ArgumentException();
				return doc.ChangeLayer(layerId, objs => objs.Add(val));
			},
			doc => doc.ChangeLayer(layerId, objs => objs.Del(curveId)),
			doc => doc.Layers.Any(e => e.Id == layerId) && doc.Layers.Get(layerId).Objects.Any(e => e.Id == curveId) && doc.Layers.Get(layerId).Objects.Get(curveId) is Curve,
			doc => (Curve)doc.Layers.Get(layerId).Objects.Get(curveId),
			(doc, val) =>
			{
				if (val.Id != curveId) throw new ArgumentException();
				return doc.ChangeLayer(layerId, objs => objs.Set(val));
			}
		);
}
*/



public sealed class Model<D> : IDisposable
{
	private readonly Disp d = MkD();
	public void Dispose() => d.Dispose();

	private readonly Undoer<D> undoer;

	public D V
	{
		get => undoer.V;
		set => undoer.V = value;
	}

	public Model(D init)
	{
		undoer = new Undoer<D>(init).D(d);
	}
}

public interface IId { Guid Id { get; } }
public interface IObj : IId;
public sealed record Curve(Guid Id, string Name): IObj { public static Curve Empty() => new(Guid.NewGuid(), "empty"); }
public sealed record Layer(Guid Id, IObj[] Objects): IObj { public static Layer Empty() => new(Guid.NewGuid(), []); }
public sealed record Doc(Layer[] Layers) { public static Doc Empty() => new([Layer.Empty()]); }


public static class DocExt
{
	public static Doc ChangeLayer(this Doc doc, Guid layerId, Func<IObj[], IObj[]> fun) => doc with { Layers = doc.Layers.Set(layerId, layer => layer with { Objects = fun(layer.Objects) }) };
}


public static class IIdExt
{
	public static T[] Add<T>(this T[] xs, T x) where T : IId
	{
		if (xs.Any(e => e.Id == x.Id)) throw new ArgumentException();
		return xs.ToList().Append(x).ToArray();
	}
	public static T[] Del<T>(this T[] xs, Guid xId) where T : IId
	{
		if (xs.All(e => e.Id != xId)) throw new ArgumentException();
		var idx = xs.Idx(xId);
		return xs.Take(idx).Concat(xs.Skip(idx + 1)).ToArray();
	}
	/*public static T[] Del<T>(this T[] xs, T x) where T : IId
	{
		if (xs.All(e => e.Id != x.Id)) throw new ArgumentException();
		var idx = xs.Idx(x);
		return xs.Take(idx).Concat(xs.Skip(idx + 1)).ToArray();
	}*/
	public static T[] Set<T>(this T[] xs, T x) where T : IId
	{
		if (xs.All(e => e.Id != x.Id)) throw new ArgumentException();
		var idx = xs.Idx(x);
		return xs.Take(idx).Append(x).Concat(xs.Skip(idx + 1)).ToArray();
	}
	public static T[] Set<T>(this T[] xs, Guid xId, Func<T, T> fun) where T : IId
	{
		if (xs.All(e => e.Id != xId)) throw new ArgumentException();
		var idx = xs.Idx(xId);
		return xs.Take(idx).Append(fun(xs[idx])).Concat(xs.Skip(idx + 1)).ToArray();
	}
	public static T Get<T>(this T[] xs, Guid xId) where T : IId
	{
		if (xs.All(e => e.Id != xId)) throw new ArgumentException();
		var idx = xs.Idx(xId);
		return xs[idx];
	}

	private static int Idx<T>(this T[] xs, T x) where T : IId => xs.Idx(x.Id);

	private static int Idx<T>(this T[] xs, Guid xId) where T : IId
	{
		for (var i = 0; i < xs.Length; i++)
			if (xs[i].Id == xId)
				return i;
		throw new ArgumentException();
	}
}



public interface IUndoer : IDisposable
{
	IObservable<Unit> WhenDo { get; }
	IObservable<Unit> WhenUndo { get; }
	IObservable<Unit> WhenRedo { get; }
	IObservable<Unit> WhenChanged { get; }
	bool Undo();
	bool Redo();
	void InvalidateRedos();
	string GetLogStr();
}

sealed class Undoer<T> : IUndoer
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly IRwVar<T> cur;
	private readonly Stack<T> stackUndo = new();
	private readonly Stack<T> stackRedo = new();

	private readonly ISubject<T> whenDo;
	private readonly ISubject<Unit> whenUndo;
	private readonly ISubject<Unit> whenRedo;
	private IObservable<T> WhenDoT => whenDo.AsObservable();
	private void Do(T v) => whenDo.OnNext(v);

	public T V
	{
		get => cur.V;
		set => Do(value);
	}

	// IUndoer
	public IObservable<Unit> WhenDo => WhenDoT.ToUnit();
	public IObservable<Unit> WhenUndo => whenUndo.AsObservable();
	public IObservable<Unit> WhenRedo => whenRedo.AsObservable();
	public IObservable<Unit> WhenChanged => cur.AsObservable().ToUnit();
	public bool Undo()
	{
		if (stackUndo.Count == 0) return false;
		whenUndo.OnNext(Unit.Default);
		return true;
	}
	public bool Redo()
	{
		if (stackRedo.Count == 0) return false;
		whenRedo.OnNext(Unit.Default);
		return true;
	}
	public void InvalidateRedos() => stackRedo.Clear();



	public Undoer(T init)
	{
		whenDo = new Subject<T>().D(d);
		whenUndo = new Subject<Unit>().D(d);
		whenRedo = new Subject<Unit>().D(d);

		cur = Var.Make(init, d);

		WhenDoT.Subscribe(v =>
		{
			stackUndo.Push(cur.V);
			stackRedo.Clear();
			cur.V = v;
		}).D(d);

		WhenUndo.Subscribe(_ =>
		{
			if (stackUndo.Count == 0) throw new ArgumentException();
			var valUndo = stackUndo.Pop();
			stackRedo.Push(cur.V);
			cur.V = valUndo;
		}).D(d);

		WhenRedo.Subscribe(_ =>
		{
			if (stackRedo.Count == 0) throw new ArgumentException();
			var valRedo = stackRedo.Pop();
			stackUndo.Push(cur.V);
			cur.V = valRedo;
		}).D(d);
	}

	public string GetLogStr()
	{
		var sb = new StringBuilder();
		foreach (var elt in stackRedo.Reverse())
			sb.AppendLine($"(redo)    {elt}");
		sb.AppendLine($"current-> {cur.V}");
		foreach (var elt in stackUndo)
			sb.AppendLine($"(undo)    {elt}");
		return sb.ToString();
	}
}



public static class RxExt
{
	public static IObservable<Unit> ToUnit<T>(this IObservable<T> source) => source.Select(_ => Unit.Default);
}












































