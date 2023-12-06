using PowRxVar;
using LinqVec;
using LinqVec.Logic;
using LinqVec.Structs;
using VectorEditor.Model;
using LinqVec.Tools;
using LinqVec.Tools.Events;
using LinqVec.Utils;
using PowMaybe;
using VectorEditor.Tools.Curve_.Mods;
using VectorEditor.Tools.Curve_.Structs;

namespace VectorEditor.Tools.Curve_;


sealed class CurveTool(ToolEnv env, ModelMan<DocModel> mm) : Tool<DocModel>(env, mm)
{
	public override Keys Shortcut => Keys.F1;

	public override async Task Run(IRoDispBase d)
	{
		var evt = Env.GetEvtForTool(this)
			.ToGrid(Env.Transform)
			.SnapToGrid(Env.Transform)
			.TrackPos(out var mousePos, d)
			.RestrictToGrid()
			.MakeHot(d)
			.ToEvt(e => Env.Curs.Cursor = e);

		evt.WhenEvt.Log(d);

		var curve = mm.Create(Entities.Curve(mm.V.Layers[0].Id));

		try
		{

			while (true)
			{
				var hot = await evt.Choose(
					Hotspots.MovePoint_Start(curve).Map(e => (IHot)new HotPointId(e)),
					Hotspots.AddPoint_Start(curve).Map(e => (IHot)new HotPt(e))
				).D(d);

				switch (hot)
				{
					case HotPt { Pos: var startPt }:
						curve.ModSet(CurveMods.AddPoint(startPt));
						await evt.Choose(Hotspots.AddPoint_Finish).D(d);
						curve.ModApply(mousePos);
						break;

					case HotPointId { PointId: var pointId }:
						curve.ModSet(CurveMods.MovePoint(pointId));
						await evt.Choose(Hotspots.MovePoint_Finish).D(d);
						curve.ModApply(mousePos);
						break;
				}
			}

		}
		catch (Exception)
		{
			curve.Invalidate();
			throw;
		}
	}
}


sealed record HotPt(Pt Pos) : IHot;
sealed record HotPointId(PointId PointId) : IHot;



static class Hotspots
{
	public static IHotspot<Pt> AddPoint_Start(IEntity<CurveModel> curve) => new PtHotspot(Trigger.Down, onOver: on => curve.ModSet(CurveMods.AddPoint().If(on)));
	public static IHotspot<Pt> AddPoint_Finish => new PtHotspot(Trigger.Up);
	public static IHotspot<PointId> MovePoint_Start(IEntity<CurveModel> curve) => new PointIdHotspot(curve);
	public static IHotspot<Pt> MovePoint_Finish => new PtHotspot(Trigger.Up, CBase.Cursors.BlackArrowSmall);

	private sealed record PtHotspot(Trigger trigger, Cursor? cursor = null, Action<bool>? onOver = null) : IHotspot<Pt>
	{
		public string Name => "Pt";
		public Cursor Cursor => cursor ?? CBase.Cursors.Pen;
		public Maybe<Pt> Get(Pt mousePos) => May.Some(mousePos);
		public Trigger Trigger => trigger;
		public Action<bool>? OnOver => onOver;
	}

	private sealed record PointIdHotspot(IEntity<CurveModel> curve) : IHotspot<PointId>
	{
		public string Name => "PointId";
		public Cursor Cursor => CBase.Cursors.BlackArrowSmall;
		public Maybe<PointId> Get(Pt mousePos) => curve.V.GetClosestPointTo(mousePos, C.ActivateMoveMouseDistance);
		public Trigger Trigger => Trigger.Down;
	}
}

