using Microsoft.Reactive.Testing;
using System.Reactive;

namespace TestLib;

public static class RxTestMakers
{
	public static long Sec(this int v) => TimeSpan.FromSeconds(v).Ticks;
	public static long Sec(this double v) => TimeSpan.FromSeconds(v).Ticks;

	public static Recorded<Notification<T>> OnNext<T>(double sec, T v) => new(sec.Sec(), Notification.CreateOnNext(v));
	public static Recorded<Notification<T>> OnError<T>(double sec, Exception ex) => new(sec.Sec(), Notification.CreateOnError<T>(ex));
	public static Recorded<Notification<T>> OnCompleted<T>(double sec) => new(sec.Sec(), Notification.CreateOnCompleted<T>());
}

public static class TestFmt
{
	public static string Fmt<T>(this Recorded<Notification<T>> e)
	{
		var tStr = $"{TimeSpan.FromTicks(e.Time).TotalSeconds:F1}s";
		var notStr = e.Value.Kind switch
		{
			//NotificationKind.OnNext => $"OnNext({e.Value.Value})",
			NotificationKind.OnNext => $"{e.Value.Value}",
			NotificationKind.OnError => $"OnError([{e.Value.Exception!.GetType().Name}] {e.Value.Exception.Message})",
			NotificationKind.OnCompleted => "OnCompleted()",
			_ => throw new ArgumentException()
		};
		return $"[{tStr}] {notStr}";
	}

	public static void LogMessages<T>(this ITestableObserver<T> obs, string title)
	{
		var msgs = obs.Messages;
		L("");
		LTitle($"{title} (x{msgs.Count})");
		foreach (var msg in msgs)
			L($"    {msg.Fmt()}");
		L("");
	}

	private static void L(string s) => Console.WriteLine(s);
	private static void LTitle(string s)
	{
		L(s);
		L(new string('=', s.Length));
	}
}