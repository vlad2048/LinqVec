using ReactiveVars;

namespace PtrLib;

public sealed class ModUserCancelledException : Exception;
public sealed record Mod<T>(
	string Name,
	IObservable<Func<T, T>> Fun
)
{
	public override string ToString() => $"Mod({Name})";
	public static readonly Mod<T> Empty = new(nameof(Empty), Obs.Empty<Func<T, T>>());
}


public interface IModEvt;

public sealed record ModStartEvt(string Name) : IModEvt
{
	public override string ToString() => $"Start({Name})";
}

public sealed record ModFinishEvt(string Name, bool Commit, string Str) : IModEvt
{
	public override string ToString() => $"{Verb}({Name})  -> {Str}";
	private string Verb => Commit ? "Commit" : "Cancel";
}



public interface IPtr<TDoc> : IDisposable
{
	IBoundVar<TDoc> V { get; }
	IRoVar<TDoc> VGfx { get; }
	IScopedPtr<TSub> Scope<TSub>(
		TSub init,
		Func<TDoc, TSub, TDoc> del,
		Func<TDoc, TSub, TDoc> add,
		Func<TSub, bool> valid
	);
	void Undo();
	void Redo();
	IObservable<Unit> WhenPaintNeeded { get; }
}

public static class Ptr
{
	public static IPtr<TDoc> Make<TDoc>(TDoc init, Disp d) => new Ptr<TDoc>(init, d);
}

public interface IScopedPtr
{
	IObservable<Unit> WhenPaintNeeded { get; }
}

public interface IScopedPtr<TSub> : IScopedPtr, IDisposable
{
	IRwVar<TSub> V { get; }
	IRoVar<TSub> VGfx { get; }
	void Commit();
	void SetMod(Mod<TSub> modV);
	IObservable<IModEvt> WhenModEvt { get; }
}
