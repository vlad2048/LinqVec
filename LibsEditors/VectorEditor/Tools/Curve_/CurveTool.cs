using LinqVec;
using LinqVec.Logic;
using VectorEditor.Model;
using LinqVec.Tools;
using LinqVec.Tools.Acts;
using LinqVec.Tools.Events;
using ReactiveVars;
using VectorEditor.Tools.Curve_.Mods;

namespace VectorEditor.Tools.Curve_;


sealed class CurveTool(ToolEnv Env, Model<Doc> Doc) : ITool
{
	public Keys Shortcut => Keys.P;

	private static class States
	{
		public const string Neutral = nameof(Neutral);
	}
	private static class Acts
	{
		public const string MovePoint = nameof(MovePoint);
		public const string AddPoint = nameof(AddPoint);
	}

	public IDisposable Run(ToolActions toolActions)
	{
		var d = MkD();
		Doc.EnableRedrawOnMouseMove(d);

		var evt = Env.GetEvtForTool(this, true, d);


		var curve = new MemMouseModder<Curve>(Curve.Empty()).D(d);
		toolActions.SetUndoer(curve.Undoer);
		var gfxState = CurveGfxState.None;

		evt.WhenKeyDown(Keys.Escape).Subscribe(_ => toolActions.Reset()).D(d);
		evt.WhenKeyDown(Keys.Enter).Subscribe(_ =>
		{
			if (curve.Get().Pts.Length > 0)
				Doc.V = Doc.V.AddObject(curve.Get());
			toolActions.Reset();
		}).D(d);



		ActSetMaker ModeNeutral(Unit _) => _ => new ActSet(
			States.Neutral,
			CBase.Cursors.Pen,
			[
				Hotspots.CurvePointButLast(curve)
					.Do(pointId => [
						Act.Drag(
							Acts.MovePoint,
							curve,
							CurveMods.MovePoint(pointId)
						)
					]),
				Hotspots.Anywhere
					.Do(_ => [
						Act.Drag(
							Acts.AddPoint,
							curve,
							CurveMods.AddPoint(Unit.Default)
						)
					]),
			]
		);

		ModeNeutral(Unit.Default)
			.Run(evt, d)
			.Subscribe(e =>
			{
				switch (e)
				{
					case HotspotHoverRunEvt { Hotspot: "Anywhere", On: true }:
						curve.ModSet(CurveMods.AddPoint());
						gfxState = CurveGfxState.AddPoint;
						break;
					case HotspotHoverRunEvt { Hotspot: "Anywhere", On: false }:
						curve.ModClear();
						gfxState = CurveGfxState.None;
						break;

					case DragStartRunEvt { Act: Acts.AddPoint }:
						gfxState = CurveGfxState.DragHandle;
						break;
					case ConfirmRunEvt { Act: Acts.AddPoint }:
						curve.ModSet(CurveMods.AddPoint());
						gfxState = CurveGfxState.AddPoint;
						break;

					case DragStartRunEvt { Act: Acts.MovePoint }:
						gfxState = CurveGfxState.Edit;
						break;
				}
			}).D(d);


		Env.WhenPaint.Subscribe(gfx =>
		{
			CurvePainter.Draw(gfx, curve.GetModded(evt.MousePos.V), gfxState);
		}).D(d);

		return d;
	}
}
