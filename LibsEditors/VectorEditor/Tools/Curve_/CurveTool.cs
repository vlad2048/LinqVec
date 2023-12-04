using System.Reactive;
using System.Reactive.Linq;
using PowRxVar;
using LinqVec;
using LinqVec.Logic;
using LinqVec.Structs;
using PowMaybe;
using VectorEditor.Model;
using VectorEditor.Tools.Curve_.Drawing;
using VectorEditor.Tools.Curve_.Mods;
using VectorEditor.Tools.Curve_.Structs;
using VectorEditor.Tools.Curve_.Utils;
using LinqVec.Tools;
using LinqVec.Tools.Events;
using LinqVec.Utils.WinForms_;

namespace VectorEditor.Tools.Curve_;


sealed class CurveTool(ToolEnv env, ModelMan<DocModel> mm) : Tool<DocModel>(env, mm)
{
	private interface ICurveState;
	private sealed record AddPointPrep : ICurveState;
	private sealed record AddPointProgress : ICurveState;
	private sealed record MovePointPrep(PointId Id) : ICurveState;
	private sealed record MovePointProgress : ICurveState;

	public override Keys Shortcut => Keys.F1;

	public override IDisposable RunRest(Action<Pt> startFun)
	{
		var d = new Disp();

		Env.Curs.Cursor = CBase.Cursors.Pen;

		var curveEvt = Env.GetEvtForTool(this)
			.ToGrid(Env.Transform)
			.SnapToGrid(Env.Transform)
			.TrackPos(out var mousePos, d)
			.RestrictToGrid()
			.MakeHot(d);

		curveEvt.WhenMouseDown()
			.Where(_ => mousePos.V.IsSome())
			.Select(_ => mousePos.V.Ensure())
			.Subscribe(startPos =>
			{
				startFun(startPos);
			}).D(d);

		Env.WhenPaint
			.Subscribe(gfx =>
			{
				gfx.DrawMouseMarker(mousePos.V);
			}).D(d);

		Obs.Merge(
			mousePos.ToUnit()
		).Subscribe(_ => Env.Invalidate()).D(d);

		return d;
	}



	public override IDisposable Run(Pt startPt)
	{
		var d = new Disp();

		var curveEvt = Env.GetEvtForTool(this)
			.ToGrid(Env.Transform)
			.SnapToGrid(Env.Transform)
			.TrackPos(out var mousePos, d)
			.RestrictToGrid()
			.MakeHot(d);

		var model = MM.CreateEdit(Entities.Curve).D(d);
		var state = Var.Make<ICurveState>(new AddPointPrep()).D(d);
		var modder = new Modder(state, mousePos, model).D(d);

		modder.CreateModAndGoto(
			new AddPointCurveMod(startPt),
			new AddPointProgress()
		);

		state.Subscribe(s => Env.Curs.Cursor = GetCursorForState(s)).D(d);

		MM.WhenUndoRedo.ObserveOnUI().Subscribe(_ => modder.DiscardModAndGoto(new AddPointPrep())).D(d);



		curveEvt.Subscribe(evt =>
		{
			Pt p;

			switch (state.V)
			{
				// ************
				// * AddPoint *
				// ************
				case AddPointPrep:
					modder.CreateMod(new AddPointCurveMod(null));
					if (evt.IsMouseMove(out p) && model.V.GetClosestPointTo(p, C.ActivateMoveMouseDistance).IsSome(out var closeId))
						modder.DiscardModAndGoto(
							new MovePointPrep(closeId)
						);
					else if (evt.IsMouseDown(out p))
						modder.CreateModAndGoto(
							new AddPointCurveMod(p),
							new AddPointProgress()
						);
					break;

				case AddPointProgress:
					if (evt.IsMouseUp())
						modder.ApplyModAndGoto(
							new AddPointPrep()
						);
					break;


				// *************
				// * MovePoint *
				// *************
				case MovePointPrep { Id: var id }:
					if (evt.IsMouseMove(out p) && (model.V.GetPointById(id) - p).Length >= C.ActivateMoveMouseDistance)
						modder.DiscardModAndGoto(
							new AddPointPrep()
						);
					else if (evt.IsMouseDown())
						modder.CreateModAndGoto(
							new MovePointCurveMod(id),
							new MovePointProgress()
						);
					else if (evt.IsKeyDown(Keys.Delete))
					{
						modder.CreateMod(new RemovePointCurveMod(id.Idx));
						modder.ApplyModAndGoto(new AddPointPrep());
					}
					break;

				case MovePointProgress:
					if (evt.IsMouseUp())
						modder.ApplyModAndGoto(
							new AddPointPrep()
						);
					break;

				default:
					throw new ArgumentException();
			}
		}).D(d);




		/*
		curveEvt.Subscribe(evt =>
		{
			switch (state.V)
			{

				// ************
				// * AddPoint *
				// ************
				case AddPointPrep:
					modder.CreateMod(new AddPointCurveMod(null));
					switch (evt)
					{
						case MouseMoveEvtGen<Pt> { Pos: var pos } when model.V.GetClosestPointTo(pos, C.ActivateMoveMouseDistance).IsSome(out var closeId):
							modder.DiscardModAndGoto(
								new MovePointPrep(closeId)
							);
							break;
						case MouseBtnEvtGen<Pt> { Pos: var pos, UpDown: UpDown.Down, Btn: MouseBtn.Left }:
							modder.CreateModAndGoto(
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
							modder.ApplyModAndGoto(
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
							modder.DiscardModAndGoto(
								new AddPointPrep()
							);
							break;
						case MouseBtnEvtGen<Pt> { Pos: _, UpDown: UpDown.Down, Btn: MouseBtn.Left }:
							modder.CreateModAndGoto(
								new MovePointCurveMod(id),
								new MovePointProgress()
							);
							break;
						case KeyEvtGen<Pt> { Key: Keys.Delete, UpDown: UpDown.Down }:
							modder.CreateMod(new RemovePointCurveMod(id.Idx));
							modder.ApplyModAndGoto(new AddPointPrep());
							break;
					}
					break;

				case MovePointProgress:
					switch (evt)
					{
						case MouseBtnEvtGen<Pt> { UpDown: UpDown.Up, Btn: MouseBtn.Left }:
							modder.ApplyModAndGoto(
								new AddPointPrep()
							);
							break;
					}
					break;

				default:
					throw new ArgumentException();
			}
		}).D(d);
		*/


		Env.WhenPaint
			.Subscribe(gfx =>
			{
				CurveModelPainter.Draw(gfx, model.V.ApplyMod(modder.Mod, mousePos.V));
				gfx.DrawMouseMarker(mousePos.V);
			}).D(d);

		Obs.Merge(
			curveEvt.ToUnit(),
			MM.WhenChanged.ToUnit(),
			modder.WhenChanged,
			mousePos.ToUnit()
		).Subscribe(_ => Env.Invalidate()).D(d);

		return d;
	}





	private sealed class Modder : IDisposable
	{
		private readonly Disp d = new();
		public void Dispose() => d.Dispose();

		private readonly IRwVar<ICurveState> state;
		private readonly IRoVar<Maybe<Pt>> mousePos;
		private readonly IRwVar<ICurveMod> mod;
		private readonly ISmartId<CurveModel> model;

		public ICurveMod Mod => mod.V;
		public IObservable<Unit> WhenChanged => mod.ToUnit();

		public Modder(
			IRwVar<ICurveState> state,
			IRoVar<Maybe<Pt>> mousePos,
			ISmartId<CurveModel> model
		)
		{
			this.state = state;
			this.mousePos = mousePos;
			this.model = model;
			mod = Var.Make<ICurveMod>(new NoneCurveMod()).D(d);
		}

		public void CreateMod(ICurveMod modNext)
		{
			mod.V = modNext;
		}

		public void CreateModAndGoto(ICurveMod modNext, ICurveState stateNext)
		{
			mod.V = modNext;
			state.V = stateNext;
		}

		public void DiscardModAndGoto(ICurveState stateNext)
		{
			mod.V = new NoneCurveMod();
			state.V = stateNext;
		}

		public void ApplyModAndGoto(ICurveState stateNext)
		{
			if (mousePos.V.IsSome(out var p))
				model.V = model.V.ApplyMod(mod.V, p);
			mod.V = new NoneCurveMod();
			state.V = stateNext;
		}
	}





	private static Cursor GetCursorForState(ICurveState s) => s switch
	{
		AddPointPrep => CBase.Cursors.Pen,
		MovePointPrep => CBase.Cursors.BlackArrowSmall,
		AddPointProgress => CBase.Cursors.BlackArrowSmall,
		MovePointProgress => CBase.Cursors.BlackArrowSmall,
		_ => throw new ArgumentException()
	};
}


