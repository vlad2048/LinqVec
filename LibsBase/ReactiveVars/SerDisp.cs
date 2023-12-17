using System.Reactive.Disposables;

namespace PowRxVar;

public sealed class SerDisp : IDisposable
{
	public void Dispose() => serD.Dispose();

	private readonly SerialDisposable serD = new();

	public Disp GetNewD()
	{
		serD.Disposable = null;
		var d = new Disp();
		serD.Disposable = d;
		return d;
	}
}