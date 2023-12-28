using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Geom;
using LinqVec.Utils.Rx;
using ReactiveVars;

namespace LinqVec;



public interface IObjDesc<Doc, O, Loc> where Doc : IDoc where O : IId
{
	O Make(Guid objId);
	bool Contains(Doc doc, Loc loc, Guid objId);
	Loc LocStrat(Doc doc);
	Doc Add(Doc doc, Loc loc, O obj);
	Doc Del(Doc doc, Loc loc, Guid objId);
	O Get(Doc doc, Loc loc, Guid objId);
	Doc Set(Doc doc, Loc loc, O obj);
}

// @formatter:off
sealed class ObjAccessor<Doc, O, Loc>(IObjDesc<Doc, O, Loc> desc) where Doc : IDoc where O : IId
{
	public O Make(Guid objId) => desc.Make(objId);
	public bool Contains(Doc doc, Loc loc, Guid objId) => desc.Contains(doc, loc, objId);
	public Loc LocStrat(Doc doc) => desc.LocStrat(doc);
	public Doc Add(Doc doc, Loc loc, O obj) { if (Contains(doc, loc, obj.Id)) throw new ArgumentException(); return desc.Add(doc, loc, obj); }
	public Doc Del(Doc doc, Loc loc, Guid objId) { if (!Contains(doc, loc, objId)) throw new ArgumentException(); return desc.Del(doc, loc, objId); }
	public O Get(Doc doc, Loc loc, Guid objId) { if (!Contains(doc, loc, objId)) throw new ArgumentException(); return desc.Get(doc, loc, objId); }
	public Doc Set(Doc doc, Loc loc, O obj) { if (!Contains(doc, loc, obj.Id)) throw new ArgumentException(); return desc.Set(doc, loc, obj); }
}
// @formatter:on


public sealed record Mod<O>(
	string Name,
	bool ApplyWhenFinished,
	IRoVar<Func<O, O>> Fun
)
{
	public override string ToString() => $"[mod:{Name}]";
	public static readonly Mod<O> Empty = new("Empty", false, Var.MakeConst(new Func<O, O>(e => e)));
}

// @formatter:off
public interface IModEvt;
public sealed record SetModEvt(string Name) : IModEvt { public override string ToString() => $"ModSet({Name})"; }
public sealed record ApplyModEvt(string Name) : IModEvt { public override string ToString() => $"ModApply({Name})"; }
// @formatter:on

public interface IPtrMod<O>
{
	IObservable<IModEvt> WhenModEvt { get; }
	O V { get; }
	O ModGet();
	void ModSet(Mod<O> mod);
}

public static class PtrExt
{
	public static Action ClearMod<O>(this IPtrMod<O> ptr) => () => ptr.ModSet(Mod<O>.Empty);
	public static Action HoverMod<O>(this IPtrMod<O> ptr, Mod<O> mod) => () => ptr.ModSet(mod);
	public static Action<Pt> DragMod<O>(this IPtrMod<O> ptr, Func<Pt, Mod<O>> mod) => pt => ptr.ModSet(mod(pt));
}

public interface IPtrDoc<out Doc>
	: IDisposable
	where Doc : IDoc
{
	IObservable<Unit> WhenPaintNeeded { get; }
	Doc Hide();
}

public interface IPtr<out Doc, O>
	: IPtrMod<O>, IPtrDoc<Doc>
	where Doc : IDoc
	where O : IId;


public sealed class PtrCreate<Doc, O, Loc>
	: IPtr<Doc, O>
	where Doc : IDoc
	where O : IId
{
	private readonly Disp d;
	public void Dispose() => d.Dispose();

	private readonly IBoundVar<Doc> doc;
	private readonly ObjAccessor<Doc, O, Loc> access;
	private readonly Guid objId;
	private readonly IRwVar<Option<Mod<O>>> mod;
	private readonly Subject<IModEvt> whenModEvt;

	private Loc loc;
	private bool isEmpty = true;


	// IPtrMod<O>
	// ==========
	public IObservable<IModEvt> WhenModEvt => whenModEvt.AsObservable();
	public O ModGet() => mod.V.Match(m => m.Fun.V(V), () => V);
	public void ModSet(Mod<O> mod_)
	{
		ModFlush();
		if (!whenModEvt.IsDisposed) whenModEvt.OnNext(new SetModEvt(mod_.Name));
		mod.V = Some(mod_);
	}
	private void ModFlush()
	{
		mod.V.IfSome(m =>
		{
			if (m.ApplyWhenFinished)
			{
				if (!whenModEvt.IsDisposed) whenModEvt.OnNext(new ApplyModEvt(m.Name));
				V = m.Fun.V(V);
			}
			mod.V = None;
		});
	}


	// IPtrDoc<Doc>
	// ============
	public IObservable<Unit> WhenPaintNeeded =>
		mod
			.Select(opt => opt.Match(
				m => m.Fun.ToUnit(),
				Obs.Never<Unit>
			))
			.Switch();
	public Doc Hide() => isEmpty switch {
		true => doc.V,
		false => access.Del(doc.V, loc, objId)
	};


	public PtrCreate(Model<Doc> model, IObjDesc<Doc, O, Loc> desc, Disp toolD)
	{
		d = toolD;
		doc = model.Cur;
		access = new ObjAccessor<Doc, O, Loc>(desc);
		loc = access.LocStrat(doc.V);
		objId = Guid.NewGuid();
		mod = Option<Mod<O>>.None.MakeSafe(d);
		whenModEvt = new Subject<IModEvt>().D(d);

		model.PtrSet(this);
		Disposable.Create(() => model.PtrClear(this)).D(d);

		doc
			.Where(e => !isEmpty && !access.Contains(e, loc, objId))
			.Subscribe(_ => isEmpty = true).D(d);
	}

	// IPtrMod<O>
	// ==========
	public O V
	{
		get => isEmpty switch {
			true => access.Make(objId),
			false => access.Get(doc.V, loc, objId)
		};
		set
		{
			if (isEmpty)
			{
				loc = access.LocStrat(doc.V);
				isEmpty = false;
				doc.V = access.Add(doc.V, loc, value);
			}
			else
			{
				doc.V = access.Set(doc.V, loc, value);
			}
		}
	}
}


/*
public sealed class PtrEdit<Doc, O, Loc> : IDisposable where Doc : IDoc where O : IId
{
	private readonly Disp d = MkD();
	public void Dispose() => d.Dispose();

	private readonly IBoundVar<Doc> doc;
	private readonly ObjAccessor<Doc, O, Loc> desc;
	private readonly Loc loc;
	private readonly Guid objId;

	public PtrEdit(Model<Doc> model, IObjDesc<Doc, O, Loc> desc, Loc loc, Guid objId)
	{
		doc = model.Cur;
		this.desc = new ObjAccessor<Doc, O, Loc>(desc);
		this.loc = loc;
		this.objId = objId;
	}
}
*/





/*
public sealed class Ptr<Doc, O, Loc> : IDisposable where Doc : IDoc where O : IId
{
	private interface IArgs { Loc Loc { get; } }
	private sealed record CreateArgs(Loc Loc) : IArgs;
	private sealed record EditArgs(Loc Loc, Guid Id) : IArgs;

	private readonly Disp d = MkD();
	public void Dispose() => d.Dispose();

	private readonly IBoundVar<Doc> doc;
	private readonly IObjDesc<Doc, O, Loc> desc;
	private readonly Loc loc;
	private readonly Guid id;
	private readonly IBoundVar<O> obj;

	public IRwVar<O> Obj => obj;

	private Ptr(Model<Doc> model, IObjDesc<Doc, O, Loc> desc, IArgs args)
	{
		doc = model.Cur;
		this.desc = desc;
		loc = args.Loc;

		bool hasContent;
		switch (args)
		{
			case CreateArgs:
				id = Guid.NewGuid();
				obj = Var.MakeBound(desc.Make(id), d);
				id = obj.V.Id;
				hasContent = desc.HasContent(obj.V);
				break;
			case EditArgs { Id: var id_ }:
				id = id_;
				obj = Var.MakeBound(desc.Get(doc.V, loc, id), d);
				hasContent = desc.HasContent(obj.V);
				if (!hasContent) throw new ArgumentException();
				break;
			default:
				throw new ArgumentException();
		}

		void Reset()
		{

		}

		// Tool changes Obj to HasContent
		obj.WhenOuter
			.Where(e => !hasContent && desc.HasContent(e))
			.Subscribe(e =>
			{
				hasContent = true;
				doc.V = desc.Add(doc.V, loc, e);
			}).D(d);

		// Doc Undo/Redo changes with Obj removed
		doc.WhenInner
			.Where(doc_ => !desc.Contains(doc_, loc, id))
			.Subscribe(e =>)
	}

	public static Ptr<Doc, O, Loc> Create(Model<Doc> doc, IObjDesc<Doc, O, Loc> desc, Loc loc) => new(doc, desc, new CreateArgs(loc));
	public static Ptr<Doc, O, Loc> Edit(Model<Doc> doc, IObjDesc<Doc, O, Loc> desc, Loc loc, Guid id) => new(doc, desc, new EditArgs(loc, id));
}
*/