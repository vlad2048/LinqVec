namespace ReactiveVars;

public interface IRoVar<out T> : IObservable<T>
{
	T V { get; }
}

public interface IRwVar<T> : IRoVar<T>
{
	new T V { get; set; }
	bool IsDisposed { get; }
}
