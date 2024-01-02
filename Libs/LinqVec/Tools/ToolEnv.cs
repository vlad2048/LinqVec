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
	private readonly Disp d = MkD("ToolEnv");
	public void Dispose() => d.Dispose();

	private readonly Ctrl drawPanel;
	private readonly IRwVar<ITool> curTool;
	private readonly IRoVar<bool> isPanZoom;
	private readonly IRoVar<Transform> transform;
	private readonly IObservable<IEvt> editorEvt;
	private readonly Action toolReset;
	private readonly ISubject<Unit> whenUndoRedo;
	private IObservable<Unit> WhenUndoRedo => whenUndoRedo.AsObservable();


    public ToolEnv(
	    Ctrl drawPanel,
	    IRwVar<ITool> curTool,
	    IRoVar<bool> isPanZoom,
	    IRoVar<Transform> transform,
	    IObservable<IEvt> editorEvt,
		Action toolReset
    )
    {
	    this.drawPanel = drawPanel;
	    this.curTool = curTool;
		this.isPanZoom = isPanZoom;
		this.transform = transform;
		this.editorEvt = editorEvt;
		this.toolReset = toolReset;
		whenUndoRedo = new Subject<Unit>().D(d);
    }


    internal void TriggerUndoRedo() => whenUndoRedo.OnNext(Unit.Default);


    public IRwVar<ITool> CurTool => curTool;
    public IRoVar<Transform> Transform => transform;
    public IObservable<Gfx> WhenPaint => drawPanel.WhenPaint;
    public void Invalidate() => drawPanel.Invalidate();
    public void ToolReset() => toolReset();


	public Evt GetEvtForTool(ITool tool, bool snap, Disp toolD) =>
	    snap switch
	    {
            false => editorEvt
	            .RestrictToTool(tool, CurTool, isPanZoom)
	            .ToGrid(Transform)
	            .ToEvt(e => drawPanel.Cursor = e, WhenUndoRedo, toolD),
            true => editorEvt
	            .RestrictToTool(tool, CurTool, isPanZoom)
	            .ToGrid(Transform)
	            .SnapToGrid()
	            .RestrictToGrid()
	            .ToEvt(e => drawPanel.Cursor = e, WhenUndoRedo, toolD),
		};
}