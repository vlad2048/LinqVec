global using static LoggingConfig.Logging_.LogUtils;
global using Obs = System.Reactive.Linq.Observable;
using LoggingConfig.Config_;
using LoggingConfig.File_;
using PowRxVar;


namespace LoggingConfig;

public static class Singletons
{
	public static IRoVar<Config> Cfg { get; private set; } = null!;


	private static void Init()
	{
		serD.Value = null;
		var d = serD.Value = new Disp();
		Cfg = ConfigWatcher.Watch(FileUtils.GetFilenameRelativeToExe("_Config", "cfg.json"), Config.Default).D(d);
	}

	private static readonly SerialDisp<Disp> serD = new SerialDisp<Disp>().DisposeOnProgramExit();
	static Singletons() => Init();
}