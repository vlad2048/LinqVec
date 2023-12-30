using System.Reactive.Linq;

namespace LinqVec.Utils;

public static class ThreadLogger
{
	private static Thread Cur => Thread.CurrentThread;

	private static int? mainThreadId;

	public static void IdentifyMainThread() => mainThreadId = Cur.ManagedThreadId;
	public static void Log(string s) => LogMsg(s);
	public static IObservable<T> DoLogThread<T>(this IObservable<T> source, string name) =>
		source
			.Do(_ => Log($"{name}    (IObservable<{typeof(T).Name}>)"));



	private static void LogMsg(string s) => Console.WriteLine($"[{ThreadStr}] - {s}");

	private static string ThreadStr => $"{Cur.ManagedThreadId}/{Cur.Name}{MainStr}".PadRight(32);
	private static string MainStr => Cur.ManagedThreadId == mainThreadId ? "(Main)" : "";
}