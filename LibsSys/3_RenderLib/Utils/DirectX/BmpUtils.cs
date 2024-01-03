using System.Drawing.Imaging;
using ReactiveVars;
using Vortice;
using Vortice.DCommon;
using PixelFormat = Vortice.DCommon.PixelFormat;

namespace RenderLib.Utils.DirectX;

static class BmpUtils
{
    public static (D2D.ID2D1Bitmap, IDisposable) Load(string filename, D2D.ID2D1DeviceContext d2dDeviceCtx, WIC.IWICImagingFactory wicFactory)
    {
        var d = new Disp();
        var decoder = wicFactory.CreateDecoderFromFileName(filename, null, FileAccess.Read, WIC.DecodeOptions.CacheOnLoad);
        var frame = decoder.GetFrame(0);
        var formatConverter = wicFactory.CreateFormatConverter();
        if (formatConverter.Initialize(frame, WIC.PixelFormat.Format32bppPBGRA, WIC.BitmapDitherType.None, null, 0.0, WIC.BitmapPaletteType.MedianCut).Failure)
            throw new InvalidOperationException("formatConverter.Initialize failed");
        var wicBmp = wicFactory.CreateBitmapFromSource(formatConverter, WIC.BitmapCreateCacheOption.CacheOnLoad);
        var d2dBmp = d2dDeviceCtx.CreateBitmapFromWicBitmap(wicBmp).D(d);
        return (d2dBmp, d);
    }


    public static D2D.ID2D1Bitmap FromBmp(D2D.ID2D1RenderTarget d2dDeviceCtx, Bitmap gdiBmp)
    {
	    var w = gdiBmp.Width;
	    var h = gdiBmp.Height;
	    var bp = new D2D.BitmapProperties(
		    new PixelFormat(DXGI.Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)
	    );

	    var data = gdiBmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
	    var stride = data.Stride;
	    using var stream = new DataStream(data.Scan0, h * stride, true, false);

	    var bmp = d2dDeviceCtx.CreateBitmap(
			new Size(w, h),
			stream.PositionPointer,
			stride,
			bp
	    );

	    gdiBmp.UnlockBits(data);

	    return bmp;
    }
}