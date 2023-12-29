using System.Reactive.Linq;
using System.Reactive.Subjects;
using LinqVec.Components.Grid_;
using LinqVec.Structs;
using LinqVec.Utils;
using LinqVec.Drawing;
using ReactiveVars;
using UILib;

namespace LinqVec.Controls;

public sealed partial class DrawPanel : UserControl
{
	private readonly ISubject<Gfx> whenPaint;

	public IObservable<Gfx> WhenPaint => whenPaint.AsObservable();


	public DrawPanel(IRwVar<Transform> transform)
	{
		DoubleBuffered = true;
		var ctrlD = this.GetD();
		var gfxResources = new GfxResources().D(ctrlD);
		whenPaint = new Subject<Gfx>().D(ctrlD);
		
		InitializeComponent();


		this.InitRX(d =>
		{
			if (DrawPanelUtils.SetupForDesignMode(DesignMode, d, this, gfxResources, transform)) return;

			this.Events().Paint.Subscribe(evt =>
			{
				var gfx = DrawPanelUtils.InitTransformAndMakeGfx(this, evt, gfxResources, transform);
				GridPainter.DrawAndSetTransform(gfx);
				whenPaint.OnNext(gfx);
			}).D(d);
		});
	}
}

file static class DrawPanelUtils
{
	public static Gfx InitTransformAndMakeGfx(Control ctrl, PaintEventArgs evt, GfxResources gfxResources, IRwVar<Transform> transform)
	{
		var clientSz = ctrl.ClientSize.ToPt();
		if (transform.V == Transform.Id)
			transform.V = Transform.MakeInitial(clientSz);
		return new Gfx(evt.Graphics, clientSz, transform.V, gfxResources);
	}

	public static bool SetupForDesignMode(bool designMode, Disp d, Control ctrl, GfxResources gfxResources, IRwVar<Transform> transform)
	{
		if (designMode)
		{
			ctrl.Events().Paint.Subscribe(evt =>
			{
				//evt.Graphics.FillRectangle(gfxResources.Brush(MkCol("681E47")), ClientRectangle);
				var gfx = InitTransformAndMakeGfx(ctrl, evt, gfxResources, transform);
				GridPainter.DrawAndSetTransform(gfx);
			}).D(d);
		}
		return designMode;
	}
}