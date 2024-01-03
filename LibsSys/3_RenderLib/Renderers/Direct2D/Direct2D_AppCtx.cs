using System.Numerics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using GeomInt;
using ReactiveVars;
using RenderLib.Structs;
using RenderLib.Utils;
using SysWinInterfaces;
using RenderLib.Utils.DirectX;

namespace RenderLib.Renderers.Direct2D;


// **********
// * AppCtx *
// **********
public sealed class Direct2D_AppCtx : IRenderAppCtxWithDispose
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	// D2D
	// ===
	public D2D.ID2D1Factory1 D2DFactory { get; }

	// DWRITE
	// ======
	public DWRITE.IDWriteFactory7 DWRITEFactory { get; }

	internal Direct2D_AppCtx()
	{
		D2DFactory = D2D.D2D1.D2D1CreateFactory<D2D.ID2D1Factory1>(D2D.FactoryType.SingleThreaded, D2D.DebugLevel.Information).D(d);
		DWRITEFactory = DWRITE.DWrite.DWriteCreateFactory<DWRITE.IDWriteFactory7>().D(d);
	}

	public IRenderWinCtx GetWinCtx(ISysWinRenderingSupport win) => new Direct2D_WinCtx(win, this);
}


// **********
// * WinCtx *
// **********
public sealed class Direct2D_WinCtx : IRenderWinCtx
{
	//private const string ImageFilename = @"C:\Dev_Explore\WinDX\_infos\images\pixart.png";

	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly SerialDisposable resizeSerialD;
	private Disp ResizeD => (Disp)(resizeSerialD.Disposable ?? throw new ArgumentException());


	private readonly ISysWinRenderingSupport win;
	internal readonly Direct2D_AppCtx appCtx;


	internal Pencils Pencils { get; private set; } = null!;

	// D2D
	// ===
	public D2D.ID2D1HwndRenderTarget D2DHwndRenderTarget { get; private set; } = null!;


	internal Direct2D_WinCtx(ISysWinRenderingSupport win, Direct2D_AppCtx appCtx)
	{
		this.win = win;
		this.appCtx = appCtx;
		resizeSerialD = new SerialDisposable().D(d);

		win.WhenInit().Subscribe(_ =>
		{
			InitResizeResources();
		}).D(d);

		win.ClientSz
			.Where(sz => sz is { X: >= 1, Y: >= 1 } && win.IsInit.V)
			.Subscribe(sz =>
			{
				Resize(sz);
			}).D(d);

		d.D(win.D);
	}


	public void Resize(Sz sz)
	{
		if (!win.IsInit.V) return;

		resizeSerialD.Disposable = null;
		InitResizeResources();
	}

	public IGfx GetGfx(bool measureOnly) => new Direct2D_Gfx(win, this, measureOnly);




	private void InitResizeResources()
	{
		resizeSerialD.Disposable = null;
		resizeSerialD.Disposable = new Disp();

		var sz = win.ClientSz.V;
		//var dpi = User32Methods.GetDpiForWindow(win.Handle);
		var renderTargetProperties = new D2D.RenderTargetProperties(
			/*D2D.RenderTargetType.Default,
			new PixelFormat(DXGI.Format.Unknown, AlphaMode.Unknown),
			dpi, dpi,
			D2D.RenderTargetUsage.None,
			D2D.FeatureLevel.Default*/
		);
		var hwndRenderTargetProperties = new D2D.HwndRenderTargetProperties
		{
			Hwnd = win.Handle,
			PixelSize = new Size(sz.X, sz.Y),
			PresentOptions = D2D.PresentOptions.None,
		};
		D2DHwndRenderTarget = appCtx.D2DFactory.CreateHwndRenderTarget(
			renderTargetProperties,
			hwndRenderTargetProperties
		).D(ResizeD);

		Pencils = new Pencils(appCtx.D2DFactory, D2DHwndRenderTarget, appCtx.DWRITEFactory).D(ResizeD);
	}
}


// *******
// * Gfx *
// *******
public sealed class Direct2D_Gfx : IGfx
{
	private const D2D.DrawTextOptions DrawTextOptions = D2D.DrawTextOptions.None;

	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly ISysWinRenderingSupport win;
	private readonly Pencils pencils;
	private readonly bool measureOnly;

	public D2D.ID2D1HwndRenderTarget T { get; }
	public D2D.ID2D1Factory1 D2DFactory { get; }
	public DWRITE.IDWriteFactory7 DWRITEFactory { get; }

	public R R { get; set; }

	internal Direct2D_Gfx(ISysWinRenderingSupport win, Direct2D_WinCtx winCtx, bool measureOnly)
	{
		this.win = win;
		this.measureOnly = measureOnly;
		R = new R(Pt.Empty, win.ClientSz.V);
		T = winCtx.D2DHwndRenderTarget;
		D2DFactory = winCtx.appCtx.D2DFactory;
		DWRITEFactory = winCtx.appCtx.DWRITEFactory;
		pencils = winCtx.Pencils;

		T.BeginDraw();
		Disposable.Create(() =>
		{
			T.EndDraw();
		}).D(d);
	}

	public bool DrawDisabled => measureOnly || R.IsDegenerate || !win.IsInit.V;

	public void PushClip(R clipR)
	{
		if (DrawDisabled) return;
	}

	public void PopClip()
	{
		if (DrawDisabled) return;
	}


	public void FillR(R r, BrushDef brush)
	{
		if (DrawDisabled) return;
		T.FillRectangle(r.ToDrawRect(), pencils.GetBrush(brush));
	}

	public void DrawR(R r, PenDef penDef)
	{
		if (DrawDisabled) return;

		var pen = pencils.GetPen(penDef);

		var f = new RectangleF(r.X + 0.5f, r.Y + 0.5f, r.Width, r.Height);

		T.DrawRectangle(f, pen.Brush, pen.Width, pen.Style);
	}

	public void DrawLine(Pt a, Pt b, PenDef penDef)
	{
		if (DrawDisabled) return;
		var pen = pencils.GetPen(penDef);

		var m = (int)pen.Width % 2 == 0 ? 0.5f : 0.0f;
		var isHorz = a.Y == b.Y;
		var isVert = a.X == b.X;
		var (ox, oy) = (isHorz, isVert) switch
		{
			(true, false) => (0.0f, 0.5f - m),
			(false, true) => (0.5f - m, 0.0f),
			_ => (0.0f, 0.0f)
		};

		var p0 = new Vector2(a.X + ox, a.Y + oy);
		var p1 = new Vector2(b.X + ox, b.Y + oy);

		T.DrawLine(p0, p1, pen.Brush, pen.Width, pen.Style);
	}

	public void DrawBmp(Bitmap bmp)
	{
		if (DrawDisabled) return;
		var d2dBmp = pencils.GetBmp(bmp);
		var rectSrc = new R(0, 0, bmp.Width, bmp.Height);
		var rectDst = rectSrc + R.Pos;
		T.DrawBitmap(
			d2dBmp,
			rectDst.ToDrawRectF(),
			1,
			D2D.BitmapInterpolationMode.NearestNeighbor,
			rectSrc.ToDrawRectF()
		);
	}



	public Sz MeasureText_(string text, FontDef fontDef)
	{
		var layout = pencils.GetFontLayout(text, fontDef);
		var sz = new Sz(
			(int)Math.Round(layout.Metrics.Width) + DXFontUtils.TextMargin.Dir(Dir.Horz),
			(int)Math.Round(layout.Metrics.Height) + DXFontUtils.TextMargin.Dir(Dir.Vert) - 1
		);
		return sz;
	}

	public void DrawText_(string text, FontDef fontDef, Color color)
	{
		if (DrawDisabled) return;
		var layout = pencils.GetFontLayout(text, fontDef);
		var brush = pencils.GetBrush(new SolidBrushDef(color));
		var pos = new Vector2(
			R.X + DXFontUtils.TextMargin.Left,
			R.Y + DXFontUtils.TextMargin.Top - 1
		);
		T.DrawTextLayout(pos, layout, brush, DrawTextOptions);
	}
}
