using LanguageExt.SomeHelp;
using LinqVec;
using LinqVec.Tools;
using LinqVec.Tools.Cmds;
using LinqVec.Tools.Cmds.Enums;
using LinqVec.Utils;
using ReactiveVars;
using System.Linq;
using Geom;
using LinqVec.Logic;
using VectorEditor.Model;
using VectorEditor.Tools.Curve_.Mods;

namespace VectorEditor.Tools.Select_;



sealed class SelectTool(ToolEnv Env, Unmod<Doc> Doc) : ITool
{
	public Keys Shortcut => Keys.Q;

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
	}

	public Disp Run(ToolActions toolActions)
	{
		var d = MkD();

		var evt = Env.GetEvtForTool(this, true, d);

		var curSel = Var.Make<Guid[]>([], d);

		ToolStateFun ModeNeutral(Unit _) => _ =>
			new ToolState(
				States.Neutral,
				CBase.Cursors.BlackArrow,
				[
					Hotspots.Object(Doc)
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
											Doc.DragMod(DocMods.MoveSelection(evt.MousePos, curSel.V, d))
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
							)
						]),
				]
			);

		
		var cmdOutput =
			ModeNeutral(Unit.Default)
				.Run(evt, Env.Invalidate, d);

		
		Env.WhenPaint.Subscribe(gfx =>
		{
			var bboxOpt =
				Doc.VModded.GetObjects(curSel.V)
					.Select(e => e.BoundingBox)
					.Union();
			Painter.PaintSelectRectangle(gfx, bboxOpt);
		}).D(d);


		/*
		Doc.EnableRedrawOnMouseMove(d);
		
		var curSel = Option<DocMouseModder<Curve>>.None;


		ToolStateFun ModeNeutral(Unit _) => _ =>
		{
			curSel = None;
			return new ToolState(
				States.Neutral,
				CBase.Cursors.BlackArrow,
				[
					Hotspots.Curve(Doc)
						.Do(curve =>
						[
							Cmd.Click(
								Acts.Select,
								ClickGesture.Click,
								() => ModeSelected(curve)
							)
						])
				]
			);
		};



		ToolStateFun ModeSelected(Curve curve) => actD =>
		{
			var (getF, setF) = Doc.GetGetSet(curve);
			curSel = new DocMouseModder<Curve>(getF, setF).D(actD);

			return new ToolState(
				States.Selected,
				CBase.Cursors.BlackArrow,
				[
					Hotspots.Curve(Doc)
						.Do(hotCurve => (hotCurve == curve) switch {
							true => [
								Cmd.Drag(
									Acts.MoveCurve,
									curSel.Ensure(),
									CurveMods.MoveCurve
								)
							],
							false => [
								Cmd.Click(
									Acts.Select,
									ClickGesture.Click,
									() => ModeSelected(hotCurve)
								)
							]
						}),
					Hotspots.Anywhere
						.Do(_ => [
							Cmd.Click(
								Acts.UnselectCurve,
								ClickGesture.Click,
								() => ModeNeutral(Unit.Default)
							)
						])
				]
			);
		};


		ModeNeutral(Unit.Default)
			.Run(evt, d);



		Env.WhenPaint.Subscribe(gfx =>
		{
			curSel.IfSome(cur => SelectPainter.DrawSelRect(gfx, cur.GetModded(evt.MousePos.V)));
		}).D(d);*/

		return d;
	}
}
