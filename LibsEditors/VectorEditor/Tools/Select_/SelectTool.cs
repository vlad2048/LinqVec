using LinqVec;
using LinqVec.Logic;
using LinqVec.Tools;
using LinqVec.Tools.Acts;
using LinqVec.Tools.Acts.Enums;
using LinqVec.Utils;
using ReactiveVars;
using VectorEditor.Model;
using VectorEditor.Tools.Curve_.Mods;

namespace VectorEditor.Tools.Select_;



sealed class SelectTool(ToolEnv Env, Model<Doc> Doc) : ITool
{
	public Keys Shortcut => Keys.Q;

	private static class States
	{
		public const string Neutral = nameof(Neutral);
		public const string Selected = nameof(Selected);
	}
	private static class Acts
	{
		public const string SelectCurve = nameof(SelectCurve);
		public const string UnselectCurve = nameof(UnselectCurve);
		public const string MoveCurve = nameof(MoveCurve);
	}

	public IDisposable Run(ToolActions toolActions)
	{
		var d = MkD();
		Doc.EnableRedrawOnMouseMove(d);

		var evt = Env.GetEvtForTool(this, true, d);
		var curSel = Option<DocMouseModder<Curve>>.None;



		ActSetMaker ModeNeutral(Unit _) => _ =>
		{
			curSel = None;
			return new ActSet(
				States.Neutral,
				CBase.Cursors.BlackArrow,
				[
					Hotspots.Curve(Doc)
						.Do(curve =>
						[
							Act.Click(
								Acts.SelectCurve,
								ClickGesture.Click,
								() => ModeSelected(curve)
							)
						])
				]
			);
		};



		ActSetMaker ModeSelected(Curve curve) => actD =>
		{
			var (getF, setF) = Doc.GetGetSet(curve);
			curSel = new DocMouseModder<Curve>(getF, setF).D(actD);

			return new ActSet(
				States.Selected,
				CBase.Cursors.BlackArrow,
				[
					Hotspots.Curve(Doc)
						.Do(hotCurve => (hotCurve == curve) switch {
							true => [
								Act.Drag(
									Acts.MoveCurve,
									curSel.Ensure(),
									CurveMods.MoveCurve
								)
							],
							false => [
								Act.Click(
									Acts.SelectCurve,
									ClickGesture.Click,
									() => ModeSelected(hotCurve)
								)
							]
						}),
					Hotspots.Anywhere
						.Do(_ => [
							Act.Click(
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
		}).D(d);

		return d;
	}
}
