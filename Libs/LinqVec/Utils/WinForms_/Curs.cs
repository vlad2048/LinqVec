using Geom;
using LinqVec.Utils.Rx;
using ReactiveVars;

namespace LinqVec.Utils.WinForms_;

public interface ICurs
{
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
}
