using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using PowRxVar;

namespace LinqVec.Utils.Rx;

public sealed class SerMay<T> : IDisposable where T : IDisposable
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly ISubject<Option<T>> whenChanged;
	private readonly SerialDisposable serD;
	private Option<T> v = None;

	public Option<T> V
	{
		get => v;
		set
		{
			serD.Disposable = null;
			if (value.IsSome)
			{
				var val = value.IfNone(() => throw new ArgumentException());
				var serDVal = MkD();
				val.D(serDVal);
				serD.Disposable = serDVal;
			}
			v = value;
			whenChanged.OnNext(v);
		}
	}

	public IObservable<Option<T>> WhenChanged => whenChanged.AsObservable();

	public SerMay()
	{
		whenChanged = new Subject<Option<T>>().D(d);
		serD = new SerialDisposable().D(d);
	}
}