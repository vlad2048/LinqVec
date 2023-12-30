using ReactiveVars;

namespace PtrLib;

public sealed record Mod<T>(
	string Name,
	bool Apply,
	IRoVar<Func<T, T>> Fun
)
{
	public override string ToString() => $"Mod({Name}) apply:{Apply}";

	public static readonly Mod<T> Empty = new("Empty", false, Var.MakeConst<Func<T, T>>(e => e));
}

public interface IPtrRegular<T>
{
	Disp D { get; }
	T V { get; set; }
	T VModded { get; }
	IDisposable ModSet(Mod<T> modVal);
}
public interface IPtrCommit<Kid> : IPtrRegular<Kid>
{
	void Commit();
}

public interface IPtr<Dad> : IPtrRegular<Dad>
{
	// Used by the Tools
	// =================
	IPtrRegular<Kid> Edit<Kid>(
		Kid init,
		Func<Dad, Kid, Dad> setFun,
		Func<Dad, Kid, Dad> removeFun,
		Disp d
	);
	IPtrCommit<Kid> Create<Kid>(
		Kid init,
		Func<Dad, Kid, Dad> setFun,
		Func<Kid, bool> validFun,
		Disp d
	);

	// Used by VecEditor for management
	// ================================
	IObservable<Unit> WhenPaintNeeded { get; }
	IObservable<Unit> WhenUndoRedo { get; }
	void Undo();
	void Redo();

	// Used by the LayoutPane
	// ======================
	IObservable<Unit> WhenValueChanged { get; }
}




public static class Ptr
{
	public static IPtr<T> Make<T>(T init, Disp d) => new PtrDad<T>(init, d);
}