using Geom;
using LinqVec.Controls;
using LinqVec.Structs;
using ReactiveVars;

namespace LinqVec.Utils.WinForms_;


public class Ctrl(DrawPanel ctrl)
{
	public Cursor Cursor
	{
		get => ctrl.Cursor;
		set => ctrl.Cursor = value;
	}
	public IObservable<Unit> WhenSizeChanged => ctrl.Events().ClientSizeChanged.ToUnit();
	public IObservable<Gfx> WhenPaint => ctrl.WhenPaint;
	public void Invalidate() => ctrl.Invalidate();
	public Pt Sz => ctrl.ClientSize.ToPt();
}
