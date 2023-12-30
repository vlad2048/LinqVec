using LinqVec;
using LinqVec.Tools;
using LinqVec.Tools.Cmds;
using LinqVec.Tools.Cmds.Enums;
using VectorEditor._Model;

namespace VectorEditor.Tools.CurveEdit_;

sealed class CurveEditTool(Keys shortcut) : ITool<Doc>
{
	public string Name => "E";
	public Bitmap? Icon => Resource.toolicon_CurveEdit;
	public Keys Shortcut => shortcut;

	private static class States
	{
		public const string Neutral = nameof(Neutral);
	}
	private static class Cmds
	{
		public const string Select = nameof(Select);
		public const string Unselect = nameof(Unselect);
	}

	public Disp Run(ToolEnv<Doc> Env, ToolActions toolActions)
	{
		var d = MkD();
		var doc = Env.Doc;
		var evt = Env.GetEvtForTool(this, true, d);

		ToolStateFun ModeNeutral() => _ => new ToolState(
			States.Neutral,
			CBase.Cursors.BlackArrow,
			[
				Hotspots.Object<Curve>(doc.V)
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
			var curveV = doc.V.GetObjects([curveId]).OfType<Curve>().First();
			var curve = doc.Edit(curveV, CurveFuns.Create_SetFun, CurveFuns.Edit_RemoveFun, stateD);
			return new ToolState(
				States.Neutral,
				CBase.Cursors.BlackArrowSmall,
				[
					Hotspots.Object(doc.V, curveId)
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
			.Run(evt, Env.Invalidate, d);

		return d;
	}
}