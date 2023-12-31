using LinqVec;
using LinqVec.Tools;
using LinqVec.Tools.Cmds;
using LinqVec.Tools.Cmds.Enums;
using LinqVec.Utils;
using ReactiveVars;
using Geom;
using VectorEditor._Model;

namespace VectorEditor.Tools.Select_;



sealed class SelectTool(Ctx ctx) : ITool
{
	public ToolNfo Nfo { get; } = new(
		"S",
		Resource.toolicon_Select,
		Keys.Q
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
		var doc = ctx.Doc;
		var evt = ctx.Env.GetEvtForTool(this, true, d);

		var curSel = Var.Make<Guid[]>([], d);

		ToolStateFun ModeNeutral() => _ =>
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

		
		ModeNeutral()
			.Run(evt, ctx.Env.Invalidate, d);


		ctx.Env.WhenPaint.Subscribe(gfx =>
		{
			var bboxOpt =
				ctx.Doc.VModded.GetObjects(curSel.V)
					.Select(e => e.BoundingBox)
					.Union();
			Painter.PaintSelectRectangle(gfx, bboxOpt);
		}).D(d);
	}
}
