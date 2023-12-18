using System.Reactive.Disposables;

namespace ReactiveVars;

public sealed class SerDisp : IDisposable
{
	public void Dispose() => serD.Dispose();

	private readonly SerialDisposable serD = new();

	public Disp GetNewD()
	{
		serD.Disposable = null;
		var d = MkD();
		serD.Disposable = d;
		return d;
	}
}