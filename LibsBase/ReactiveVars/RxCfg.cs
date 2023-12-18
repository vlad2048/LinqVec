using System.Reactive.Linq;
using PowBasics.Files;
using PowBasics.Json_;

namespace ReactiveVars;

public static class RxCfg
{
	private static readonly TimeSpan DebounceTime = TimeSpan.FromMilliseconds(500);

	public static IRoVar<C> Make<C>(string filename, C defaultValue, Jsoner jsoner, Disp d) =>
		Obs.Create<C>(obs =>
			{
				var obsD = new Disp(); // TODO

				var init = jsoner.LoadOrCreateDefault(filename.MakeFolderForFileIFN(), defaultValue);
				obs.OnNext(init);

				var (folder, name) = filename.SplitFilename();
				var fsWatch = new FileSystemWatcher(folder, name)
				{
					NotifyFilter = NotifyFilters.LastWrite,
				}.D(obsD);

				fsWatch.WhenChanged()
					.Throttle(DebounceTime)
					.Select(_ => jsoner.Load<C>(filename))
					.Retry()
					.Subscribe(obs.OnNext).D(obsD);

				fsWatch.EnableRaisingEvents = true;

				return obsD;
			})
			.ToVar(d);
}


file static class RxCfgExt
{
	public static (string, string) SplitFilename(this string file) => (
		Path.GetDirectoryName(file) ?? throw new ArgumentException(),
		Path.GetFileName(file)
	);

	public static IObservable<FileSystemEventArgs> WhenChanged(this FileSystemWatcher watcher) => Obs.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(e => watcher.Changed += e, e => watcher.Changed -= e).Select(e => e.EventArgs);
}
