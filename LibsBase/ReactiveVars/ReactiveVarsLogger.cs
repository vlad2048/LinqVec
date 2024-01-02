using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;

namespace ReactiveVars;

public static class ReactiveVarsLogger
{
	public static void Write(string s) => Console.Write(s);
	public static void WriteLine(string s) => Console.WriteLine(s);
	public static void WriteLine() => Console.WriteLine();

	public static void IdentifyMainThread() => mainThreadId = Cur.ManagedThreadId;
	public static void LogThread(string s) => LogMsg(s);
	public static IObservable<T> DoLogThread<T>(this IObservable<T> source, string name) =>
		source
			.Do(_ => LogThread($"{name}    (IObservable<{typeof(T).Name}>)"));

	public static IObservable<T> LogIf<T>(this IObservable<T> obs, Func<bool> predicate, [CallerArgumentExpression(nameof(obs))] string? obsStr = null)
	{
		obs.Do(v =>
		{
			if (!predicate()) return;
			WriteLine($"{obsStr} <- {v}");
		});
		return obs;
	}

	/*public static O LogTimeIf<O, T>(this O obs, Func<bool> predicate, IScheduler scheduler, [CallerArgumentExpression(nameof(obs))] string? obsStr = null) where O : IObservable<T>
	{
		obs.Do(v =>
		{
			if (!predicate()) return;
			WriteLine($"[{scheduler:HH:mm:ss.fffffff}] {obsStr} -> {v}");
		});
		return obs;
	}*/









	/*public static IRoVar<T> LogTimeIf<T>(this IRoVar<T> roVar, Func<bool> predicate, Func<T, string> fmt, Disp d, IScheduler scheduler, [CallerArgumentExpression(nameof(roVar))] string? obsStr = null)
	{
		roVar.Subscribe(v =>
		{
			if (!predicate()) return;
			WriteLine($"[{scheduler:HH:mm:ss.fffffff}] {obsStr} -> {fmt(v)}");
		}).D(d);
		return roVar;
	}

	public static IObservable<T> LogTimeIf<T>(this IObservable<T> roVar, Func<bool> predicate, Func<T, string> fmt, Disp d, IScheduler scheduler, [CallerArgumentExpression(nameof(roVar))] string? obsStr = null)
	{
		roVar.Subscribe(v =>
		{
			if (!predicate()) return;
			WriteLine($"[{scheduler:HH:mm:ss.fffffff}] {obsStr} -> {fmt(v)}");
		}).D(d);
		return roVar;
	}

	public static IObservable<T> LogTimeIf<T>(this IObservable<T> roVar, Func<bool> predicate, Disp d, IScheduler scheduler, [CallerArgumentExpression(nameof(roVar))] string? obsStr = null)
	{
		roVar.Subscribe(v =>
		{
			if (!predicate()) return;
			WriteLine($"[{scheduler:HH:mm:ss.fffffff}] {obsStr} -> {v}");
		}).D(d);
		return roVar;
	}

	public static IObservable<T> LogTimeIf<T>(this IObservable<T> roVar, Func<bool> predicate, IScheduler scheduler, [CallerArgumentExpression(nameof(roVar))] string? obsStr = null)
	{
		roVar.Subscribe(v =>
		{
			if (!predicate()) return;
			WriteLine($"[{scheduler:HH:mm:ss.fffffff}] {obsStr} -> {v}");
		});
		return roVar;
	}


	public static O LogTimeIf<O, T>(this O obs, Func<bool> predicate, Func<T, string> fmt, Disp d, IScheduler scheduler, [CallerArgumentExpression(nameof(obs))] string? obsStr = null) where O : IObservable<T>
	{
		obs.Subscribe(v =>
		{
			if (!predicate()) return;
			WriteLine($"[{scheduler:HH:mm:ss.fffffff}] {obsStr} -> {fmt(v)}");
		}).D(d);
		return obs;
	}*/


	public static IObservable<T> Log<T>(this IObservable<T> obs, Disp d, [CallerArgumentExpression(nameof(obs))] string? obsStr = null)
	{
		Disposable.Create(() => WriteLine($"{obsStr} <- Dispose()")).D(d);
		obs.Subscribe(v => WriteLine($"{obsStr} <- {v}")).D(d);
		return obs;
	}

	public static IDisposable LogD<T>(this IObservable<T> obs, [CallerArgumentExpression(nameof(obs))] string? obsStr = null) =>
		obs.Subscribe(v => WriteLine($"{obsStr} <- {v}"));

	public static void Log(this Disp d, string name)
	{
		WriteLine($"[{name}].new()");
		Disposable.Create(() => WriteLine($"[{name}].Dispose()")).D(d);
	}

	public static Func<IDisposable> Log(this Func<IDisposable> fun, string name) => () =>
	{
		WriteLine($"[{name}].On");
		var d = fun();
		return Disposable.Create(() =>
		{
			WriteLine($"[{name}].Off");
			d.Dispose();
		});
	};

	public static IDisposable Log(this IDisposable d, string name)
	{
		WriteLine($"[{name}].ctor()");
		var wrapD = MkD($"log({name})");
		d.D(wrapD);
		Disposable.Create(() => WriteLine($"[{name}].Dispose()")).D(wrapD);
		return wrapD;
	}







	private static Thread Cur => Thread.CurrentThread;
	private static int? mainThreadId;
	private static void LogMsg(string s) => Console.WriteLine($"[{ThreadStr}] - {s}");
	private static string ThreadStr => $"{Cur.ManagedThreadId}/{Cur.Name}{MainStr}".PadRight(32);
	private static string MainStr => Cur.ManagedThreadId == mainThreadId ? "(Main)" : "";
}