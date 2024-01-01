using LinqVec;
using LinqVec.Tools;
using LinqVec.Tools.Cmds;
using LinqVec.Tools.Cmds.Enums;
using ReactiveVars;
using VectorEditor._Model;

namespace VectorEditor.Tools.CurveEdit_;

sealed class CurveEditTool(Ctx ctx) : ITool
{
	public ToolNfo Nfo { get; } = new(
		"E",
		Resource.toolicon_CurveEdit,
		Keys.F2
	);

	private static class States
	{
		public const string Neutral = nameof(Neutral);
	}
	private static class Cmds
	{
		public const string Select = nameof(Select);
		public const string Unselect = nameof(Unselect);
	}

	public void Run(Disp d)
	{
		var doc = ctx.Doc;
		var evt = ctx.Env.GetEvtForTool(this, true, d);

		ToolStateFun ModeNeutral() => _ => new ToolState(
			States.Neutral,
			CBase.Cursors.BlackArrow,
			[
				Hotspots.Object<Curve>(doc.V.V)
					.Do(curveId => [
						Cmd.ClickRet(
							Cmds.Select,
							ClickGesture.Click,
							() => Some(ModeSelected(curveId))
						)
					]),
			]
		);


		ToolStateFun ModeSelected(Guid curveId) => stateD =>
		{
			var curveV = doc.V.V.GetObjects([curveId]).OfType<Curve>().First();
			//var curve = doc.Edit(curveV, CurveFuns.Create_SetFun, CurveFuns.Edit_RemoveFun, stateD);
			var curve = doc.Scope(curveV, CurveFuns.Edit_RemoveFun, CurveFuns.Create_SetFun, CurveFuns.Create_ValidFun).D(stateD);
			return new ToolState(
				States.Neutral,
				CBase.Cursors.BlackArrowSmall,
				[
					Hotspots.Object(doc.V.V, curveId)
						.Do(_ => []),

					Hotspots.Anywhere
						.Do(_ => [
							Cmd.ClickRet(
								Cmds.Unselect,
								ClickGesture.Click,
								() => Some(ModeNeutral())
							)
						])
				]
			);
		};



		ModeNeutral()
			.Run(evt, ctx.Env.Invalidate, d);
	}
}