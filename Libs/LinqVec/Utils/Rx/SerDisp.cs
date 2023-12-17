using System.Reactive.Disposables;
using PowRxVar;

namespace LinqVec.Utils.Rx;

sealed class SerDisp : IDisposable
{
	public void Dispose() => serD.Dispose();

	private readonly SerialDisposable serD = new();

	public IRoDispBase GetNewD()
	{
		serD.Disposable = null;
		var d = new Disp();
		serD.Disposable = d;
		return d;
	}
}