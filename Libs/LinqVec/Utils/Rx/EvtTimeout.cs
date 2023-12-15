/*
using PowRxVar;
using PowRxVar.Utils;

namespace LinqVec.Utils.Rx;

sealed class EvtTimeout : IDisposable
{
	private static readonly TimeSpan Delay = TimeSpan.FromMilliseconds(500);

	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly SerialDisp<IRwDispBase> serD;
	private readonly Action<Unit> sendTimeout;

	public IObservable<Unit> WhenTimeout { get; }
	public void Schedule()
	{
		serD.Value = null;
		serD.Value = new Disp();
		Obs.Timer(Delay, Rx.Sched).Subscribe(_ => sendTimeout(Unit.Default)).D(serD.Value);
	}
	public void Cancel() => serD.Value = null;

	public EvtTimeout()
	{
		serD = new SerialDisp<IRwDispBase>().D(d);
		(sendTimeout, WhenTimeout) = RxEventMaker.Make<Unit>().D(d);
	}
}
*/
