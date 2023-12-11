using System.Reactive;
using System.Reactive.Disposables;
using Geom;
using PowRxVar;

namespace LinqVec.Utils.WinForms_;

public interface ICurs
{
	//IDisposable SetToolCurs(Cursor curs);
	Cursor Cursor { set; }
}

public class Ctrl(Control ctrl) : ICurs
{
	public Cursor Cursor
	{
		get => ctrl.Cursor;
		set => ctrl.Cursor = value;
	}

	public IObservable<Unit> WhenSizeChanged => ctrl.Events().ClientSizeChanged.ToUnit();

	public Pt Sz => ctrl.ClientSize.ToPt();

	public IDisposable SetToolCurs(Cursor curs)
	{
		Cursor = curs;
		return Disposable.Create(() => Cursor = Cursors.Default);
	}
}
