﻿using System.Reactive.Linq;
using System.Reactive.Subjects;
using PowRxVar;
using LinqVec.Utils.WinForms_;
using LinqVec.Components.Grid_;
using LinqVec.Structs;
using LinqVec.Utils;
using LinqVec.Drawing;

namespace LinqVec.Controls;

sealed partial class DrawPanel : UserControl
{
	private readonly ISubject<DrawPanelInitNfo> whenInit;
	private readonly ISubject<Gfx> whenPaint;
	private IObservable<DrawPanelInitNfo> WhenInit => whenInit.AsObservable();
	public IObservable<Gfx> WhenPaint => whenPaint.AsObservable();

	public void Init(DrawPanelInitNfo initNfo)
	{
		whenInit.OnNext(initNfo);
		whenInit.OnCompleted();
	}

	public DrawPanel()
	{
		DoubleBuffered = true;
		whenInit = new AsyncSubject<DrawPanelInitNfo>().D(this);
		whenPaint = new Subject<Gfx>().D(this);
		InitializeComponent();

		this.InitRX(d =>
		{
			WhenInit.Subscribe(initNfo =>
			{
				var (transform, res) = initNfo;
				if (DrawPanelUtils.SetupForDesignMode(DesignMode, d, this, res, transform)) return;

				Obs.Merge(
						transform.ToUnit()
					)
					.Subscribe(_ => Invalidate()).D(d);

				this.Events().Paint.Subscribe(evt =>
				{
					var gfx = DrawPanelUtils.InitTransformAndMakeGfx(this, evt, res, transform);
					GridPainter.DrawAndSetTransform(gfx);
					whenPaint.OnNext(gfx);
				}).D(d);
			}).D(d);
		});
	}
}

file static class DrawPanelUtils
{
	public static Gfx InitTransformAndMakeGfx(Control ctrl, PaintEventArgs evt, Res res, IRwVar<Transform> transform)
	{
		var clientSz = ctrl.ClientSize.ToPtInt();
		if (transform.V == Transform.Id)
			transform.V = Transform.MakeInitial(clientSz);
		return new Gfx(evt.Graphics, clientSz, transform.V, res);
	}

	public static bool SetupForDesignMode(bool designMode, IRoDispBase d, Control ctrl, Res res, IRwVar<Transform> transform)
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