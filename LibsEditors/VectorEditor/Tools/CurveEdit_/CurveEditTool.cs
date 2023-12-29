using LinqVec;
using LinqVec.Tools;
using LinqVec.Tools.Cmds;
using LinqVec.Tools.Events;
using VectorEditor.Model;
using VectorEditor.Tools.Curve_.Mods;

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
		public const string ShiftSelect = nameof(ShiftSelect);
		public const string MoveSelection = nameof(MoveSelection);
		public const string UnselectAll = nameof(UnselectAll);
		public const string Delete = nameof(Delete);
	}

	public Disp Run(ToolEnv<Doc> Env, ToolActions toolActions)
	{
		var d = MkD();
		var doc = Env.Doc;
		var evt = Env.GetEvtForTool(this, true, d);

		ToolStateFun ModeNeutral(Unit _) => _ => new ToolState(
			States.Neutral,
			CBase.Cursors.BlackArrow,
			[
				Hotspots.Object<Curve>(doc)
					.Do(curveId => [

					]),
			]
		);
		ModeNeutral(Unit.Default)
			.Run(evt, Env.Invalidate, d);

		return d;
	}
}