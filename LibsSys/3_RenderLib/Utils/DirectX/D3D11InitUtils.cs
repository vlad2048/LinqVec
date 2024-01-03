using System.Diagnostics;
using System.Reactive.Disposables;
using ReactiveVars;
using RenderLib.Renderers.Direct2DInDirect3D;

namespace RenderLib.Utils.DirectX;


static class D3D11InitUtils
{
    public static (TDev, TCtx, TDXGIDev, IDisposable) Helper_D3D11CreateDevice<TDev, TCtx, TDXGIDev>()
        where TDev : D3D11.ID3D11Device
        where TCtx : D3D11.ID3D11DeviceContext
        where TDXGIDev : DXGI.IDXGIDevice
        =>
            Helper_D3D11CreateDevice<TDev, TCtx, TDXGIDev>(
                null,
                RConsts.DriverType,
                RConsts.CreationFlags,
                RConsts.FeatureLevels
            );

    private static (TDev, TCtx, TDXGIDev, IDisposable) Helper_D3D11CreateDevice<TDev, TCtx, TDXGIDev>(
        DXGI.IDXGIAdapter? adapter,
        D3D.DriverType driverType,
        D3D11.DeviceCreationFlags creationFlags,
        D3D.FeatureLevel[] featureLevels
    )
        where TDev : D3D11.ID3D11Device
        where TCtx : D3D11.ID3D11DeviceContext
        where TDXGIDev : DXGI.IDXGIDevice
    {
        var d = new Disp();
        if (D3D11.D3D11.D3D11CreateDevice(
                adapter,
                driverType,
                creationFlags,
                featureLevels,
                out var d3dDeviceTemp,
                out var d3dDeviceCtxTemp
            ).Failure)
            throw new InvalidOperationException("D3D11CreateDevice failed");
        var d3dDevice = d3dDeviceTemp.QueryInterface<TDev>().D(d);
        var d3dDeviceCtx = d3dDeviceCtxTemp.QueryInterface<TCtx>().D(d);
        d3dDeviceCtxTemp.Dispose();
        d3dDeviceTemp.Dispose();

        var dxgiDev = d3dDevice.QueryInterface<TDXGIDev>().D(d);

        Disposable.Create(() =>
        {
            static void D(string s) => Debug.WriteLine(s);
            static void DTitle(string s)
            {
                D("");
                D(s);
                D(new string('=', s.Length));
            }

            DTitle("DirectX Leaks (ignore the ID3D11Device & ID3D11Context lines)");
            var d3dDebug = d3dDevice.QueryInterfaceOrNull<D3D11.Debug.ID3D11Debug>();
            if (d3dDebug == null)
            {
                Console.WriteLine("  Failed to retrieve the Debug interface");
            }
            else
            {
                d3dDebug.ReportLiveDeviceObjects(D3D11.Debug.ReportLiveDeviceObjectFlags.Detail | D3D11.Debug.ReportLiveDeviceObjectFlags.IgnoreInternal);
                d3dDebug.Dispose();
            }
            D("");

        }).D(d);

        return (d3dDevice, d3dDeviceCtx, dxgiDev, d);
    }


    public static (DXGI.IDXGISwapChain1, IDisposable) Helper_CreateSwapChainForHwnd(
        DXGI.IDXGIFactory2 DXGIFactory,
        D3D11.ID3D11Device1 D3DDevice,
        nint hwnd,
        Sz sz
    )
    {
        var d = new Disp();

        /*var swapChainDesc = new DXGI.SwapChainDescription1
		{
			Width = sz.Width,
			Height = sz.Height,
			Format = DXGI.Format.B8G8R8A8_UNorm,
			BufferCount = 2,
			BufferUsage = DXGI.Usage.RenderTargetOutput,
			SampleDescription = DXGI.SampleDescription.Default,
			Scaling = DXGI.Scaling.Stretch,
			SwapEffect = DXGI.SwapEffect.FlipDiscard,
			AlphaMode = DXGI.AlphaMode.Ignore,
			Flags = DXGI.SwapChainFlags.None,
		};*/

        var swapChainDesc = new DXGI.SwapChainDescription1
        {
            Width = sz.X,
            Height = sz.Y,
            Format = DXGI.Format.B8G8R8A8_UNorm,
            BufferCount = 2,
            BufferUsage = DXGI.Usage.RenderTargetOutput,
            SampleDescription = DXGI.SampleDescription.Default,
            Scaling = DXGI.Scaling.None,
            SwapEffect = DXGI.SwapEffect.FlipDiscard,
            AlphaMode = DXGI.AlphaMode.Ignore,
            Flags = DXGI.SwapChainFlags.None,
        };

        var swpChainFullScreenDesc = new DXGI.SwapChainFullscreenDescription
        {
            Windowed = true,
        };
        var SwapChain = DXGIFactory.CreateSwapChainForHwnd(
            D3DDevice,
            hwnd,
            swapChainDesc,
            swpChainFullScreenDesc
        ).D(d);


        DXGIFactory.MakeWindowAssociation(hwnd, DXGI.WindowAssociationFlags.IgnoreAltEnter);


        return (SwapChain, d);
    }
}