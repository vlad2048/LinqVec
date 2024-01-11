namespace ReactiveVars;

public interface IRoVar<out T> : IObservable<T>
{
	T V { get; }
}

public interface IRwVar<T> : IRoVar<T>, IHasDisp
{
	new T V { get; set; }
}

/*								IObservable		WhenOuter	WhenInner
	-----------------------------------------------------------------
	x = Var.MakeBound(1, d);	1
	x.V = 2;					2				2
	x.SetInner(3);				3							3
*/
public interface IBoundVar<T> : IRwVar<T>
{
	IObservable<T> WhenOuter { get; }
	IObservable<T> WhenInner { get; }
	void SetInner(T v);
}
