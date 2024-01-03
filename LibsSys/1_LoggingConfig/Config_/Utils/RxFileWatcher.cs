using PowRxVar;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace LoggingConfig.Config_.Utils;



sealed class RxFileWatcher : IDisposable
{
	private static readonly TimeSpan DebounceTime = TimeSpan.FromMilliseconds(500);

	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly string file;
	private readonly FileSystemWatcher watcher;

	public IObservable<string> WhenChanged { get; }


	public RxFileWatcher(string file)
	{
		this.file = file;
		var (folder, name) = file.SplitFilename();

		watcher = new FileSystemWatcher(folder, name)
		{
			NotifyFilter = NotifyFilters.LastWrite
		}.D(d);

		WhenChanged = watcher.WhenChanged()
				.Throttle(DebounceTime)
				.Select(_ => Read())
				.Switch()
				.Retry()
			/*.Catch<string, Exception>(ex =>
			{
				L($"-------> Config loading FAILURE: {ex.Message}");
				return Obs.Never<string>();
			})*/
			;

		Disposable.Create(() => watcher.EnableRaisingEvents = false).D(d);
	}

	public void Start()
	{
		watcher.EnableRaisingEvents = true;
		File.SetLastWriteTime(file, DateTime.Now);
	}

	private async Task<string> Read()
	{
		try
		{
			var str = await File.ReadAllTextAsync(file);
			L("Config Loaded");
			return str;
		}
		catch (Exception ex)
		{
			L($"Config Loading Failed: {ex.GetType().Name} / {ex.Message}");
			throw;
		}
	}
}



file static class RxFileWatcherExt
{
	public static (string, string) SplitFilename(this string file) => (
		Path.GetDirectoryName(file) ?? throw new ArgumentException(),
		Path.GetFileName(file)
	);

	public static IObservable<FileSystemEventArgs> WhenChanged(this FileSystemWatcher watcher) => Obs.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(e => watcher.Changed += e, e => watcher.Changed -= e).Select(e => e.EventArgs);
}
