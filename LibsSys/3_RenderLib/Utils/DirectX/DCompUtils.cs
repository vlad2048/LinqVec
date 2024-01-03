using ReactiveVars;
using Vortice;

namespace RenderLib.Utils.DirectX;

static class DCompUtils
{
    public static (DCOMP.IDCompositionSurface, IDisposable) Helper_CreateAndPaintSurface(
        this DCOMP.IDCompositionDevice dcompDevice,
        SizeF sz,
        int dpi,
        D2D.ID2D1DeviceContext d2dDeviceCtx,
        Action<D2D.ID2D1DeviceContext, Pt> paintAction
    )
    {
        using var d = new Disp();

        var (dcompSurface, returnD) = dcompDevice.Helper_CreateSurface(sz);

        d2dDeviceCtx.Target =
            d2dDeviceCtx.Helper_CreateBitmapFromDxgiSurface(
                dcompSurface.Helper_BeginDraw(sz, out var ofs).D(d),
                dpi
            ).D(d);
        d2dDeviceCtx.BeginDraw();

        paintAction(d2dDeviceCtx, ofs);

        d2dDeviceCtx.EndDraw();
        dcompSurface.EndDraw();

        return (dcompSurface, returnD);
    }



    private static (DCOMP.IDCompositionSurface, IDisposable) Helper_CreateSurface(this DCOMP.IDCompositionDevice dcompDevice, SizeF sz)
    {
        var d = new Disp();
        if (dcompDevice.CreateSurface((int)sz.Width, (int)sz.Height, DXGI.Format.R8G8B8A8_UNorm, DXGI.AlphaMode.Ignore, out var dcompSurface).Failure)
            throw new InvalidOperationException("DCompDevice.CreateSurface failed");
        dcompSurface.D(d);
        return (dcompSurface, d);
    }

    private static (DXGI.IDXGISurface, IDisposable) Helper_BeginDraw(this DCOMP.IDCompositionSurface dcompSurface, SizeF sz, out Pt ofs)
    {
        var d = new Disp();
        var r = new RawRect(0, 0, (int)sz.Width, (int)sz.Height);
        if (dcompSurface.BeginDraw<DXGI.IDXGISurface>(r, out var dxgiSurface, out var ofsRaw).Failure || dxgiSurface == null)
            throw new InvalidOperationException("dcompSurface.BeginDraw failed");
        dxgiSurface.D(d);
        ofs = new Pt(ofsRaw.X, ofsRaw.Y);
        return (dxgiSurface, d);
    }

    private static (D2D.ID2D1Bitmap1, IDisposable) Helper_CreateBitmapFromDxgiSurface(this D2D.ID2D1DeviceContext d2dDeviceCtx, DXGI.IDXGISurface dxgiSurface, int dpi)
    {
        var d = new Disp();
        var bmpProps = new D2D.BitmapProperties1(
            new Vortice.DCommon.PixelFormat(DXGI.Format.R8G8B8A8_UNorm, Vortice.DCommon.AlphaMode.Ignore),
            dpi,
            dpi,
            D2D.BitmapOptions.Target | D2D.BitmapOptions.CannotDraw
        );
        var d2dTarget = d2dDeviceCtx.CreateBitmapFromDxgiSurface(dxgiSurface, bmpProps).D(d);
        return (d2dTarget, d);
    }
}