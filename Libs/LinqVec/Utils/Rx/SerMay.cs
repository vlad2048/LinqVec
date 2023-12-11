using System.Reactive.Linq;
using System.Reactive.Subjects;
using PowMaybe;
using PowRxVar;

namespace LinqVec.Utils.Rx;

public sealed class SerMay<T> : IDisposable where T : IDisposable
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly ISubject<Maybe<T>> whenChanged;
	private readonly SerialDisp<IDisposable> serD;
	private Maybe<T> v = May.None<T>();

	public Maybe<T> V
	{
		get => v;
		set
		{
			serD.Value = null;
			if (value.IsSome(out var val))
			{
				var serDVal = new Disp();
				val.D(serDVal);
				serD.Value = serDVal;
			}
			v = value;
			whenChanged.OnNext(v);
		}
	}

	public IObservable<Maybe<T>> WhenChanged => whenChanged.AsObservable();

	public SerMay()
	{
		whenChanged = new Subject<Maybe<T>>().D(d);
		serD = new SerialDisp<IDisposable>().D(d);
	}
}