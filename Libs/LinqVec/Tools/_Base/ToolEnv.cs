using LinqVec.Controls;
using LinqVec.Structs;
using LinqVec.Tools._Base.Events;
using LinqVec.Utils.WinForms_;
using PowRxVar;

namespace LinqVec.Tools._Base;

public interface IToolEnv
{
    ICurs Curs { get; }
    IRoVar<Transform> Transform { get; }
    void Invalidate();
    IObservable<Gfx> WhenPaint { get; }
    IObservable<IEvtGen<PtInt>> GetEvtForTool(Tool tool);
}

sealed class ToolEnv(
        DrawPanel drawPanel,
        ICurs curs,
        IRoVar<Tool> curTool,
        IRoVar<bool> isPanZoom,
        IRoVar<Transform> transform,
        IObservable<IEvtGen<PtInt>> editorEvt
    )
    : IToolEnv
{
    public ICurs Curs { get; } = curs;
    public IRoVar<Transform> Transform { get; } = transform;
    public void Invalidate() => drawPanel.Invalidate();
    public IObservable<Gfx> WhenPaint { get; } = drawPanel.WhenPaint;

    public IObservable<IEvtGen<PtInt>> GetEvtForTool(Tool tool) =>
        editorEvt
            .RestrictToTool(tool, curTool, isPanZoom);
}