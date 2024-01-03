using RenderLib.Renderers;
using RenderLib.Renderers.Direct2D;
using RenderLib.Renderers.Direct2DInDirect3D;
using RenderLib.Renderers.GDIPlus;
using SysWinInterfaces;

namespace RenderLib;

public static class RendererGetter
{
	public static IRenderWinCtx Get(RendererType type, ISysWinRenderingSupport win) => Get(type).GetWinCtx(win);

	// TODO
	//private static readonly Lazy<IRenderAppCtxWithDispose> gdiplusAppCtx = new(() => new GDIPlus_AppCtx().DisposeOnProgramExit());
	//private static readonly Lazy<IRenderAppCtxWithDispose> direct2dAppCtx = new(() => new Direct2D_AppCtx().DisposeOnProgramExit());
	//private static readonly Lazy<IRenderAppCtxWithDispose> direct2dindirect3dAppCtx = new(() => new Direct2DInDirect3D_AppCtx().DisposeOnProgramExit());


	private static readonly Lazy<IRenderAppCtxWithDispose> gdiplusAppCtx = new(() => new GDIPlus_AppCtx());
	private static readonly Lazy<IRenderAppCtxWithDispose> direct2dAppCtx = new(() => new Direct2D_AppCtx());
	private static readonly Lazy<IRenderAppCtxWithDispose> direct2dindirect3dAppCtx = new(() => new Direct2DInDirect3D_AppCtx());

	private static IRenderAppCtx Get(RendererType type) => type switch
	{
		RendererType.GDIPlus => gdiplusAppCtx.Value,
		RendererType.Direct2D => direct2dAppCtx.Value,
		RendererType.Direct2DInDirect3D => direct2dindirect3dAppCtx.Value,
		_ => throw new ArgumentException()
	};
}