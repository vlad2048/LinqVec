using LoggingConfig.Config_.Utils;
using PowRxVar;
using System.Reactive.Linq;

namespace LoggingConfig.Config_;

public static class ConfigWatcher
{
	public static (IRoVar<CFG>, IDisposable) Watch<CFG>(string file, CFG defaultConfig)
	{
		var d = new Disp();

		if (!File.Exists(file))
			JsonUtils.SaveFile(file, defaultConfig);

		var fileWatcher = new RxFileWatcher(file).D(d);

		var cfg = Var.Make(
			defaultConfig,
			fileWatcher.WhenChanged.Select(str => JsonUtils.LoadStr(str, file, defaultConfig))
		).D(d);

		fileWatcher.Start();

		return (cfg, d);
	}
}