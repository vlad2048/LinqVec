using LinqVec.Controls;
using LinqVec.Structs;
using LinqVec.Tools.Events;
using LinqVec.Utils.WinForms_;
using PowRxVar;

namespace LinqVec.Tools;

public sealed class ToolEnv(
        DrawPanel drawPanel,
        ICurs curs,
        IRoVar<Tool> curTool,
        IRoVar<bool> isPanZoom,
        IRoVar<Transform> transform,
        IObservable<IEvtGen<PtInt>> editorEvt,
        Action setNoneTool
    )
{
    public ICurs Curs { get; } = curs;
    public IRoVar<Transform> Transform { get; } = transform;
    public void Invalidate() => drawPanel.Invalidate();
    public IObservable<Gfx> WhenPaint { get; } = drawPanel.WhenPaint;

    public IObservable<IEvtGen<PtInt>> EditorEvt => editorEvt;
    public IObservable<IEvtGen<PtInt>> GetEvtForTool(Tool tool) => editorEvt.RestrictToTool(tool, curTool, isPanZoom);

    public void SetNoneTool() => setNoneTool();
}