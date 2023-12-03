using LinqVec.Drawing;
using LinqVec.Tools._Base;
using LinqVec.Tools._Base.Events;
using LinqVec.Tools._Base.Utils;
using LinqVec.Tools.Curve_.Model;
using PowMaybe;
using PowRxVar;

namespace LinqVec.Tools.Curve_;


public sealed class CurveTool : Tool
{
	private interface ICurveState;
	private sealed record AddPointPrep : ICurveState;
	private sealed record AddPointProgress : ICurveState;
	private sealed record MovePointPrep(PointId Id) : ICurveState;
	private sealed record MovePointProgress : ICurveState;

    public override string Name => "curve";
    public override Keys Shortcut => Keys.F1;
    public override (Tool, IDisposable) Init(IToolEnv env)
    {
        var d = new Disp();

        var mod = Var.Make<ICurveMod>(new NoneCurveMod()).D(d);

        var curveEvt = env.GetEvtForTool(this)
            .ToGrid(env.Transform)
            .SnapToGrid(env.Transform)
            .TrackPos(out var mousePos, d)
            .RestrictToGrid()
            .MakeHot(d);

        var model = new Undoer<CurveModel>(CurveModel.Empty, curveEvt).D(d);

		var state = Var.Make<ICurveState>(new AddPointPrep()).D(d);

		state.Subscribe(s => env.Curs.Cursor = s switch
        {
            AddPointPrep => C.Cursors.Pen,
            MovePointPrep => C.Cursors.BlackArrowSmall,
            AddPointProgress => C.Cursors.BlackArrowSmall,
            MovePointProgress => C.Cursors.BlackArrowSmall,
            _ => throw new ArgumentException()
        }).D(d);


		void CreateModAndGoto(ICurveMod modNext, ICurveState stateNext)
		{
			mod.V = modNext;
			state.V = stateNext;
		}

		void DiscardModAndGoto(ICurveState stateNext)
		{
			mod.V = new NoneCurveMod();
			state.V = stateNext;
		}

		void ApplyModAndGoto(ICurveState stateNext)
		{
			if (mousePos.V.IsSome(out var p))
				model.Do(model.V.ApplyMod(mod.V, p));
			mod.V = new NoneCurveMod();
			state.V = stateNext;
		}


		curveEvt.Subscribe(evt =>
        {
	        switch (state.V)
	        {

		        // ************
		        // * AddPoint *
		        // ************
		        case AddPointPrep:
			        mod.V = new AddPointCurveMod(null);
			        switch (evt)
			        {
				        case MouseMoveEvtGen<Pt> { Pos: var pos } when model.V.GetClosestPointTo(pos, C.ActivateMoveMouseDistance).IsSome(out var closeId):
					        DiscardModAndGoto(
						        new MovePointPrep(closeId)
							);
					        break;
				        case MouseBtnEvtGen<Pt> { Pos: var pos, UpDown: UpDown.Down, Btn: MouseBtn.Left }:
					        CreateModAndGoto(
						        new AddPointCurveMod(pos),
						        new AddPointProgress()
							);
					        break;
			        }

			        break;

		        case AddPointProgress:
			        switch (evt)
			        {
				        case MouseBtnEvtGen<Pt> { UpDown: UpDown.Up, Btn: MouseBtn.Left }:
					        ApplyModAndGoto(
						        new AddPointPrep()
							);
					        break;
			        }
			        break;


		        // *************
		        // * MovePoint *
		        // *************
		        case MovePointPrep { Id: var id }:
			        switch (evt)
			        {
				        case MouseMoveEvtGen<Pt> { Pos: var pos } when (model.V.GetPointById(id) - pos).Length >= C.ActivateMoveMouseDistance:
					        DiscardModAndGoto(
						        new AddPointPrep()
							);
					        break;
				        case MouseBtnEvtGen<Pt> { Pos: _, UpDown: UpDown.Down, Btn: MouseBtn.Left }:
					        CreateModAndGoto(
						        new MovePointCurveMod(id),
						        new MovePointProgress()
							);
					        break;
						case KeyEvtGen<Pt> { Key: Keys.Delete, UpDown: UpDown.Down }:
							mod.V = new RemovePointCurveMod(id.Idx);
							ApplyModAndGoto(new AddPointPrep());
							break;
			        }
			        break;

		        case MovePointProgress:
			        switch (evt)
			        {
						case MouseBtnEvtGen<Pt> { UpDown: UpDown.Up, Btn: MouseBtn.Left }:
					        ApplyModAndGoto(
						        new AddPointPrep()
							);
							break;
			        }
			        break;

		        default:
			        throw new ArgumentException();
	        }
        }).D(d);


        env.WhenPaint
            .Subscribe(gfx =>
            {
                CurveModelPainter.Draw(gfx, model.V.ApplyMod(mod.V, mousePos.V));
                gfx.DrawMouseMarker(mousePos.V);
            }).D(d);

        Obs.Merge(
	        curveEvt.ToUnit(),
	        model.WhenChanged,
	        mod.ToUnit(),
			mousePos.ToUnit()
        ).Subscribe(_ => env.Invalidate()).D(d);

		return (this, d);
    }
}