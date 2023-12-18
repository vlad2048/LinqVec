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
				"Neutral",
				CBase.Cursors.BlackArrow,
				[
					Hotspots.Curve(Doc)
						.Do(curve =>
						[
							Act.Click(
								"SelectCurve",
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
				"Selected",
				CBase.Cursors.BlackArrow,
				[
					Hotspots.Curve(Doc)
						.Do(hotCurve => (hotCurve == curve) switch {
							true => [
								Act.Drag(
									"MoveCurve",
									curSel.Ensure(),
									CurveMods.MoveCurve
									//false
								)
							],
							false => [
								Act.Click(
									"SelectCurve",
									ClickGesture.Click,
									() => ModeSelected(hotCurve)
								)
							]
						}),
					Hotspots.Anywhere
						.Do(_ => [
							Act.Click(
								"UnselectCurve",
								ClickGesture.Click,
								() => ModeNeutral(Unit.Default)
							)
						])
				]
			);
		};


		ModeNeutral(Unit.Default)
			.Run(evt, d);




		/*ActMaker ModeNeutral(Pt _) => _ =>
		{
			curSel = None;
			return new(
				"Neutral",
				CBase.Cursors.BlackArrow,
				[
					Act.Click(
						SelectActIds.SelectCurve,
						ClickGesture.Click,
						Hotspots.Curve(Doc),
						ModeSelected
					)
				]
			);
		};

		ActMaker ModeSelected(Curve curve) => actD =>
		{
			//curSel = curve;
			var (getF, setF) = Doc.GetGetSet(curve);
			curSel = new DocMouseModder<Curve>(getF, setF).D(actD);

			return new(
				"Selected",
				CBase.Cursors.BlackArrow,
				[
					Act.Click(
						SelectActIds.SelectCurve,
						ClickGesture.Click,
						Hotspots.CurveExcept(Doc, curve),
						ModeSelected
					),

					Act.DragMod(
						SelectActIds.MoveCurve,
						Hotspots.CurveSpecific(Doc, curve).ToPt(),
						curSel.IfNone(() => throw new ArgumentException()),
						CurveMods.MoveCurve,
						false
					),

					Act.Click(
						SelectActIds.Noop,
						ClickGesture.Click,
						Hotspots.CurveSpecific(Doc, curve),
						_ => None
					),

					Act.Click(
						SelectActIds.UnselectCurve,
						ClickGesture.Click,
						Hotspots.Anywhere,
						ModeNeutral
					),
				]
			);
		};

		ModeNeutral(Pt.Zero)
			.Run(evt, d);*/


		Env.WhenPaint.Subscribe(gfx =>
		{
			curSel.IfSome(cur => SelectPainter.DrawSelRect(gfx, cur.GetModded(evt.MousePos.V)));
		}).D(d);

		return d;
	}
}


static class SelectActIds
{
	public const string Noop = nameof(Noop);
	public const string SelectCurve = nameof(SelectCurve);
	public const string UnselectCurve = nameof(UnselectCurve);
	public const string MoveCurve = nameof(MoveCurve);
}