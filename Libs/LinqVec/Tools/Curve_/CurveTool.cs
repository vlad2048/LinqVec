using System.Reactive.Linq;
using LinqVec.Drawing;
using LinqVec.Tools._Base;
using LinqVec.Tools._Base.Events;
using LinqVec.Tools.Curve_.Events;
using LinqVec.Tools.Curve_.Model;
using PowRxVar;

namespace LinqVec.Tools.Curve_;

public sealed class CurveTool : Tool
{
    public override string Name => "curve";
    public override Keys Shortcut => Keys.F1;
    public override (Tool, IDisposable) Init(IToolEnv env)
    {
        var d = new Disp();

        env.Curs.SetToolCurs(C.Cursors.Pen).D(d);

        var model = new CurveModel().D(d);

        var curveEvt = env.GetEvtForTool(this)
            .ToGrid(env.Transform)
            .SnapToGrid(env.Transform)
            .TrackPos(out var mousePos, d)
            .RestrictToGrid()
            .MakeHot(d)
            .ToCurveEvt(out var curveState, d);

        curveEvt.OfType<ClickCurveEvt>().Subscribe(e => model.AddClickPoint(e.Pos)).D(d);
        curveEvt.OfType<DragStartCurveEvt>().Subscribe(e => model.AddClickPoint(e.Pos)).D(d);
        curveEvt.OfType<DragMoveCurveEvt>().Subscribe(e => model.UpdateHandles(e.Pos)).D(d);

        Obs.Merge(
            curveEvt.ToUnit(),
            model.WhenChanged,
            mousePos.ToUnit()
        ).Subscribe(_ => env.Invalidate()).D(d);

        env.WhenPaint
            .Subscribe(gfx =>
            {
                CurveModelPainter.Draw(gfx, model, curveState, mousePos);
                gfx.DrawMarker(mousePos.V, MarkerType.CurvePointProgress);
            }).D(d);

        return (this, d);
    }
}