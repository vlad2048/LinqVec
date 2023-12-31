using LinqVec;
using LinqVec.Tools;
using LinqVec.Tools.Cmds;
using LinqVec.Tools.Cmds.Enums;
using LinqVec.Utils;
using ReactiveVars;
using Geom;
using VectorEditor._Model;

namespace VectorEditor.Tools.Select_;



sealed class SelectTool(Ctx c) : ITool
{
	public ToolNfo Nfo { get; } = new(
		"S",
		Resource.toolicon_Select,
		Keys.F1
	);

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

	public void Run(Disp d)
	{
		var evt = c.Env.GetEvtForTool(this, true, d);


		ToolStateFun ModeNeutral() => _ =>
			new ToolState(
				States.Neutral,
				CBase.Cursors.BlackArrow,
				[
					Hotspots.Object(c.Doc.V)
						.Do(objId => [
								Cmd.Click(
									Cmds.Select,
									ClickGesture.Click,
									() => c.State.Select([objId])
								),
								Cmd.Click(
									Cmds.ShiftSelect,
									ClickGesture.ShiftClick,
									() => c.State.SelectF(e => e.Toggle(objId))
								),
								.. c.State.V.Selection.Contains(objId)
									? new[] {
										Cmd.Drag(
											Cmds.MoveSelection,
											c.Doc.ModSetDrag(Cmds.MoveSelection, (ptStart, ptEnd, docV) => docV.MoveSelection(c.State.V.Selection, ptEnd - ptStart))
										)
									}
									: [],
							]
						),
					Hotspots.Anywhere
						.Do(_ => [
							Cmd.Click(
								Cmds.UnselectAll,
							ClickGesture.Click,
								() => c.State.Select([])
							),
						]),
				],
				[
					Kbd.Make(
						Cmds.Delete,
						Keys.Delete,
						() => c.Doc.V = c.Doc.V.DeleteObjects(c.State.V.Selection)
					)
				]
			);

		
		ModeNeutral()
			.Run(evt, c.Env.Invalidate, d);
	}
}
