using PowRxVar;
using LinqVec;
using LinqVec.Logic;
using PowMaybe;
using VectorEditor.Model;
using VectorEditor.Tools.Curve_.Drawing;
using VectorEditor.Tools.Curve_.Mods;
using VectorEditor.Tools.Curve_.Structs;
using VectorEditor.Tools.Curve_.Utils;
using LinqVec.Tools;
using LinqVec.Tools.Events;

namespace VectorEditor.Tools.Curve_;

public sealed class CurveTool : Tool
{
	private interface ICurveState;
	private sealed record AddPointPrep : ICurveState;
	private sealed record AddPointProgress : ICurveState;
	private sealed record MovePointPrep(PointId Id) : ICurveState;
	private sealed record MovePointProgress : ICurveState;

	private readonly ModelMan<DocModel> mm;

	public override string Name => "curve";
	public override Keys Shortcut => Keys.F1;

	public CurveTool(ModelMan<DocModel> mm)
	{
		this.mm = mm;
	}

	public override (Tool, IDisposable) Init(ToolEnv env)
	{
		var d = new Disp();

		var mod = Var.Make<ICurveMod>(new NoneCurveMod()).D(d);

		var curveEvt = env.GetEvtForTool(this)
			.ToGrid(env.Transform)
			.SnapToGrid(env.Transform)
			.TrackPos(out var mousePos, d)
			.RestrictToGrid()
			.MakeHot(d);

		var model = mm.CreateEdit(Entities.Curve).D(d);

		var state = Var.Make<ICurveState>(new AddPointPrep()).D(d);

		state.Subscribe(s => env.Curs.Cursor = s switch
		{
			AddPointPrep => CBase.Cursors.Pen,
			MovePointPrep => CBase.Cursors.BlackArrowSmall,
			AddPointProgress => CBase.Cursors.BlackArrowSmall,
			MovePointProgress => CBase.Cursors.BlackArrowSmall,
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
				model.V = model.V.ApplyMod(mod.V, p);
			mod.V = new NoneCurveMod();
			state.V = stateNext;
		}

		mm.WhenUndoRedo.Subscribe(_ =>
		{
			DiscardModAndGoto(new AddPointPrep());
		}).D(d);

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
			//model.WhenChanged,
			mm.WhenChanged.ToUnit(),
			mod.ToUnit(),
			mousePos.ToUnit()
		).Subscribe(_ => env.Invalidate()).D(d);

		return (this, d);
	}
}