using LinqVec.Controls;
using LinqVec.Structs;
using LinqVec.Tools.Events;
using LinqVec.Tools.Events.Utils;
using LinqVec.Utils.WinForms_;
using PowRxVar;

namespace LinqVec.Tools;

public sealed class ToolEnv(
        DrawPanel drawPanel,
        ICurs curs,
        IRoVar<ITool> curTool,
        IRoVar<bool> isPanZoom,
        IRoVar<Transform> transform,
        IObservable<IEvt> editorEvt
	)
{
    public ICurs Curs { get; } = curs;
    public IRoVar<Transform> Transform { get; } = transform;
    public void Invalidate() => drawPanel.Invalidate();
    public IObservable<Gfx> WhenPaint { get; } = drawPanel.WhenPaint;

    public IObservable<IEvt> EditorEvt => editorEvt;
    public Evt GetEvtForTool(ITool tool, bool snap, IRoDispBase d) =>
	    snap switch
	    {
            false => editorEvt
	            .RestrictToTool(tool, curTool, isPanZoom)
	            .ToGrid(Transform)
	            .ToEvt(e => Curs.Cursor = e, d),
            true => editorEvt
	            .RestrictToTool(tool, curTool, isPanZoom)
	            .ToGrid(Transform)
	            .SnapToGrid()
	            .RestrictToGrid()
	            .ToEvt(e => Curs.Cursor = e, d),
		};
}