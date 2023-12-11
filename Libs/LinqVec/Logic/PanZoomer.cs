using System.Reactive.Linq;
using LinqVec.Structs;
using LinqVec.Tools.Events;
using LinqVec.Utils;
using LinqVec.Utils.WinForms_;
using PowBasics.MathCode;
using PowRxVar;

namespace LinqVec.Logic;

static class PanZoomer
{
	public static (IRoVar<bool>, IDisposable) Setup(
		IObservable<IEvtGen<PtInt>> evt,
		Ctrl ctrl,
		IRwVar<Transform> transform
	)
	{
		var d = new Disp();

		var isOn = evt.IsKeyDown(C.KeyMap.PanZoom, d);

		isOn.Subscribe(e => ctrl.Cursor = e ? CBase.Cursors.HandOpened : Cursors.Default).D(d);

		isOn
			.SubscribeWithDisp((on, onD) =>
			{
				if (!on) return;
				var isPanning = SetupPanning(evt, transform, ctrl).D(onD);
				isPanning.Subscribe(panning => ctrl.Cursor = panning ? CBase.Cursors.HandClosed : CBase.Cursors.HandOpened).D(onD);
			}).D(d);

		SetupZooming(evt, transform, ctrl).D(d);

		ctrl.WhenSizeChanged.Subscribe(_ => transform.V = transform.V.Cap(ctrl)).D(d);

		return (isOn, d);
	}


	private static (IRoVar<bool>, IDisposable) SetupPanning(
		IObservable<IEvtGen<PtInt>> evt,
		IRwVar<Transform> transform,
		Ctrl ctrl
	)
	{
		var d = new Disp();
		var lastMousePt = PtInt.Zero;
		var isPanning = Var.Make(false).D(d);

		evt.WhenMouseDown()
			.Where(_ => !isPanning.V)
			.Subscribe(e =>
			{
				isPanning.V = true;
				lastMousePt = e;
			}).D(d);

		evt.WhenMouseUp()
			.Where(_ => isPanning.V)
			.Subscribe(_ =>
			{
				isPanning.V = false;
				lastMousePt = PtInt.Zero;
			}).D(d);

		evt.WhenMouseMove()
			.Where(_ => isPanning.V)
			.Subscribe(e =>
			{
				var delta = -lastMousePt + (lastMousePt = e);
				var centerNext = transform.V.Center + delta.ToFloat();
				transform.V = (transform.V with { Center = centerNext }).Cap(ctrl);
			}).D(d);
		return (isPanning, d);
	}


	private static IDisposable SetupZooming(
		IObservable<IEvtGen<PtInt>> evt,
		IRwVar<Transform> transform,
		Ctrl ctrl
	)
	{
		var d = new Disp();

		evt.WhenMouseWheel().Subscribe(e =>
		{
			var zoomIndexPrev = transform.V.ZoomIndex;
			var zoomIndexNext = MathUtils.Cap(zoomIndexPrev + e.Delta, 0, C.ZoomLevels.Length - 1);
			if (zoomIndexNext == zoomIndexPrev) return;
			var p = e.Pos.ToFloat();
			var zoomPrev = transform.V.ZoomBase * C.ZoomLevels[zoomIndexPrev];
			var zoomNext = transform.V.ZoomBase * C.ZoomLevels[zoomIndexNext];
			var centerPrev = transform.V.Center;
			var centerNext = p - (p - centerPrev) * (zoomNext / zoomPrev);
			transform.V = new Transform(transform.V.ZoomBase, zoomIndexNext, centerNext).Cap(ctrl);
		}).D(d);

		return d;
	}
}



file static class PanZoomerExts
{
	public static Transform Cap(this Transform t, Ctrl ctrl)
	{
		var bbox = C.Grid.BBox().ToPixel(t);
		var (vMin, vMax) = (
			new Pt(0, 0),
			ctrl.Sz.ToFloat()
		);
		var adj = new Pt(
			bbox.Width <= ctrl.Sz.X ? 0 : (bbox.Width - ctrl.Sz.X) / 2f,
			bbox.Height <= ctrl.Sz.Y ? 0 : (bbox.Height - ctrl.Sz.Y) / 2f
		);
		var centerBox = new R(
			vMin - adj,
			vMax + adj
		);
		return t with { Center = t.Center.Cap(centerBox) };
	}

	private static Pt Cap(this Pt p, R r) => new(
		p.X.Cap(r.Min.X, r.Max.X),
		p.Y.Cap(r.Min.Y, r.Max.Y)
	);
	private static float Cap(this float v, float min, float max) => Math.Max(min, Math.Min(max, v));
}
