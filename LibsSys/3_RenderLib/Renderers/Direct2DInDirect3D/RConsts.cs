namespace RenderLib.Renderers.Direct2DInDirect3D;

public static class RConsts
{
	// D3D11.D3D11.D3D11CreateDevice
	// =============================
	public static readonly D3D.DriverType DriverType = D3D.DriverType.Hardware;
	public static readonly D3D11.DeviceCreationFlags CreationFlags = D3D11.D3D11.SdkLayersAvailable() switch
	{
		false => D3D11.DeviceCreationFlags.BgraSupport,
		true => D3D11.DeviceCreationFlags.BgraSupport | D3D11.DeviceCreationFlags.Debug,
	};
	public static readonly D3D.FeatureLevel[] FeatureLevels =
	{
		D3D.FeatureLevel.Level_11_1,
		D3D.FeatureLevel.Level_11_0,
		D3D.FeatureLevel.Level_10_1,
		D3D.FeatureLevel.Level_10_0
	};


}