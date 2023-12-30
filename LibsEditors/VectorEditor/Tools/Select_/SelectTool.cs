using LinqVec;
using LinqVec.Tools;
using LinqVec.Tools.Cmds;
using LinqVec.Tools.Cmds.Enums;
using LinqVec.Utils;
using ReactiveVars;
using Geom;
using VectorEditor.Model;

namespace VectorEditor.Tools.Select_;



sealed class SelectTool(Keys shortcut) : ITool<Doc>
{
	public string Name => "S";
	public Bitmap? Icon => Resource.toolicon_Select;
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

		var curSel = Var.Make<Guid[]>([], d);

		ToolStateFun ModeNeutral(Unit _) => _ =>
			new ToolState(
				States.Neutral,
				CBase.Cursors.BlackArrow,
				[
					Hotspots.Object(doc.V)
						.Do(objId => [
								Cmd.Click(
									Cmds.Select,
									ClickGesture.Click,
									() => curSel.V = [objId]
								),
								Cmd.Click(
									Cmds.ShiftSelect,
									ClickGesture.ShiftClick,
									() => curSel.V = curSel.V.ToggleArr(objId)
								),
								.. curSel.V.Contains(objId)
									? new[] {
										Cmd.Drag(
											Cmds.MoveSelection,
											//doc.DragMod(DocMods.MoveSelection(evt.MousePos, curSel.V, d))
											doc.ModSetDrag(Cmds.MoveSelection, (ptStart, ptEnd, docV) => docV.MoveSelection(curSel.V, ptEnd - ptStart))
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
								() => curSel.V = []
							),
						]),
				],
				[
					Kbd.Make(
						Cmds.Delete,
						Keys.Delete,
						() => doc.V = doc.V.DeleteObjects(curSel.V)
					)
				]
			);

		
		ModeNeutral(Unit.Default)
			.Run(evt, Env.Invalidate, d);

		
		Env.WhenPaint.Subscribe(gfx =>
		{
			var bboxOpt =
				Env.Doc.VModded.GetObjects(curSel.V)
					.Select(e => e.BoundingBox)
					.Union();
			Painter.PaintSelectRectangle(gfx, bboxOpt);
		}).D(d);

		return d;
	}
}
