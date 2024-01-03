namespace LoggingConfig;

public sealed record CfgLog(
	bool Redraws = false
);

public sealed record CfgOptions(
	int Renderer = 0  // 0=GDIPlus 1=Direct2D 2=Direct2DInDirect3D
);

public sealed record CfgTweaks(
	bool DisableHover = false,
	bool DisableWinSpectorDrawing = false
);


public sealed record Config(
	CfgLog Log,
	CfgOptions Options,
	CfgTweaks Tweaks
)
{
	public static Config Default = new(
		new CfgLog(),
		new CfgOptions(),
		new CfgTweaks()
	);
}
