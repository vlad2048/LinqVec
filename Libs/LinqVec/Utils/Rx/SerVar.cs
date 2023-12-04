using System.Reactive;
using PowRxVar;

namespace LinqVec.Utils.Rx;

public class SerVar<T> : IRwVar<T> where T : class, IDisposable
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();
	public IObservable<Unit> WhenDisposed => d.WhenDisposed;
	public bool IsDisposed => d.IsDisposed;

	private readonly SerialDisp<T> serD;
	private readonly IRwVar<T> rxVar;

	public T V
	{
		get => rxVar.V;
		set
		{
			serD.Value = null;
			rxVar.V = serD.Value = value;
		}
	}

	public SerVar(T initVal)
	{
		serD = new SerialDisp<T>().D(d);
		rxVar = Var.Make(serD.Value = initVal).D(d);
	}

	public IDisposable Subscribe(IObserver<T> observer) => rxVar.Subscribe(observer);
	public void OnNext(T value) => rxVar.OnNext(value);
	public void OnCompleted() => rxVar.OnCompleted();
	public void OnError(Exception error) => rxVar.OnError(error);
}