using System.Reactive.Linq;
using System.Reactive.Subjects;
using LinqVec.Controls;
using LinqVec.Structs;
using LinqVec.Tools.Events;
using LinqVec.Tools.Events.Utils;
using LinqVec.Utils.WinForms_;
using ReactiveVars;

namespace LinqVec.Tools;

public sealed class ToolEnv : IDisposable
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly DrawPanel drawPanel;
	private readonly IRoVar<ITool> curTool;
	private readonly IRoVar<bool> isPanZoom;
	private readonly IObservable<IEvt> editorEvt;
	private readonly ISubject<Unit> whenUndoRedo;
	private IObservable<Unit> WhenUndoRedo => whenUndoRedo.AsObservable();

	public ToolEnv(
		DrawPanel drawPanel,
		ICurs curs,
		IRoVar<ITool> curTool,
		IRoVar<bool> isPanZoom,
		IRoVar<Transform> transform,
		IObservable<IEvt> editorEvt
	)
	{
		this.drawPanel = drawPanel;
		this.curTool = curTool;
		this.isPanZoom = isPanZoom;
		this.editorEvt = editorEvt;
		Curs = curs;
		Transform = transform;
		WhenPaint = drawPanel.WhenPaint;
		whenUndoRedo = new Subject<Unit>().D(d);
	}

	internal void TriggerUndoRedo() => whenUndoRedo.OnNext(Unit.Default);

	public ICurs Curs { get; }
    public IRoVar<Transform> Transform { get; }
    public void Invalidate() => drawPanel.Invalidate();
    public IObservable<Gfx> WhenPaint { get; }

    public IObservable<IEvt> EditorEvt => editorEvt;
    public Evt GetEvtForTool(ITool tool, bool snap, Disp toolD) =>
	    snap switch
	    {
            false => editorEvt
	            .RestrictToTool(tool, curTool, isPanZoom)
	            .ToGrid(Transform)
	            .ToEvt(e => Curs.Cursor = e, WhenUndoRedo, toolD),
            true => editorEvt
	            .RestrictToTool(tool, curTool, isPanZoom)
	            .ToGrid(Transform)
	            .SnapToGrid()
	            .RestrictToGrid()
	            .ToEvt(e => Curs.Cursor = e, WhenUndoRedo, toolD),
		};
}