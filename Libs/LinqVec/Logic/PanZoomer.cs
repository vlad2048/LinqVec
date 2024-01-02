using System.Reactive.Linq;
using Geom;
using LinqVec.Structs;
using LinqVec.Tools.Events;
using LinqVec.Utils.WinForms_;
using PowBasics.MathCode;
using ReactiveVars;

namespace LinqVec.Logic;

static class PanZoomer
{
	public static IRoVar<bool> Setup(
		IObservable<IEvt> evt,
		Ctrl ctrl,
		IRwVar<Transform> transform,
		Disp d
	)
	{
		var isOn = evt.IsKeyDown(C.KeyMap.PanZoom);

		isOn.Subscribe(e => ctrl.Cursor = e ? CBase.Cursors.HandOpened : Cursors.Default).D(d);

		var serDisp = new SerDisp().D(d);

		isOn
			.Subscribe(on =>
			{
				var serD = serDisp.GetNewD();
				if (!on) return;
				var isPanning = SetupPanning(evt, transform, ctrl, serD);
				isPanning.Subscribe(panning => ctrl.Cursor = panning ? CBase.Cursors.HandClosed : CBase.Cursors.HandOpened).D(serD);
			}).D(d);

		SetupZooming(evt, transform, ctrl).D(d);

		ctrl.WhenSizeChanged.Subscribe(_ => transform.V = transform.V.Cap(ctrl)).D(d);

		return isOn;
	}


	private static IRoVar<bool> SetupPanning(
		IObservable<IEvt> evt,
		IRwVar<Transform> transform,
		Ctrl ctrl,
		Disp d
	)
	{
		var lastMousePt = Pt.Zero;
		var isPanning = Var.Make(false, d);

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
				lastMousePt = Pt.Zero;
			}).D(d);

		evt.WhenMouseMove()
			.Where(_ => isPanning.V)
			.Subscribe(e =>
			{
				var delta = -lastMousePt + (lastMousePt = e);
				var centerNext = transform.V.Center + delta;
				transform.V = (transform.V with { Center = centerNext }).Cap(ctrl);
			}).D(d);

		return isPanning;
	}


	private static IDisposable SetupZooming(
		IObservable<IEvt> evt,
		IRwVar<Transform> transform,
		Ctrl ctrl
	)
	{
		var d = MkD("PanZoomer");

		evt.WhenMouseWheel().Subscribe(e =>
		{
			var zoomIndexPrev = transform.V.ZoomIndex;
			var zoomIndexNext = MathUtils.Cap(zoomIndexPrev + e.Delta, 0, C.ZoomLevels.Length - 1);
			if (zoomIndexNext == zoomIndexPrev) return;
			var p = e.Pos;
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
			ctrl.Sz
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
