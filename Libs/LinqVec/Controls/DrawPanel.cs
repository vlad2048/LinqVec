using System.Reactive.Linq;
using System.Reactive.Subjects;
using LinqVec.Components.Grid_;
using LinqVec.Structs;
using LinqVec.Utils;
using LinqVec.Drawing;
using LinqVec.Utils.Rx;
using UILib;
using PowRxVar;

namespace LinqVec.Controls;

public sealed partial class DrawPanel : UserControl
{
	private readonly ISubject<DrawPanelInitNfo> whenInit;
	private readonly ISubject<Gfx> whenPaint;
	private IObservable<DrawPanelInitNfo> WhenInit => whenInit.AsObservable();

	internal void Init(DrawPanelInitNfo initNfo)
	{
		whenInit.OnNext(initNfo);
		whenInit.OnCompleted();
	}


	public IObservable<Gfx> WhenPaint => whenPaint.AsObservable();


	public DrawPanel()
	{
		DoubleBuffered = true;
		whenInit = new AsyncSubject<DrawPanelInitNfo>().D(this);
		whenPaint = new Subject<Gfx>().D(this);
		InitializeComponent();

		this.InitRX(WhenInit, (init, d) =>
		{
			var (transform, res) = init;
			if (DrawPanelUtils.SetupForDesignMode(DesignMode, d, this, res, transform)) return;

			Obs.Merge(
					transform.ToUnitExt()
				)
				.Subscribe(_ => Invalidate()).D(d);

			this.Events().Paint.Subscribe(evt =>
			{
				var gfx = DrawPanelUtils.InitTransformAndMakeGfx(this, evt, res, transform);
				GridPainter.DrawAndSetTransform(gfx);
				whenPaint.OnNext(gfx);
			}).D(d);
		});
	}
}

file static class DrawPanelUtils
{
	public static Gfx InitTransformAndMakeGfx(Control ctrl, PaintEventArgs evt, Res res, IRwVar<Transform> transform)
	{
		var clientSz = ctrl.ClientSize.ToPt();
		if (transform.V == Transform.Id)
			transform.V = Transform.MakeInitial(clientSz);
		return new Gfx(evt.Graphics, clientSz, transform.V, res);
	}

	public static bool SetupForDesignMode(bool designMode, Disp d, Control ctrl, Res res, IRwVar<Transform> transform)
	{
		if (designMode)
		{
			ctrl.Events().Paint.Subscribe(evt =>
			{
				//evt.Graphics.FillRectangle(res.Brush(MkCol("681E47")), ClientRectangle);
				var gfx = InitTransformAndMakeGfx(ctrl, evt, res, transform);
				GridPainter.DrawAndSetTransform(gfx);
			}).D(d);
		}
		return designMode;
	}
}