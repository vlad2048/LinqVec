using System.Reactive.Disposables;
using System.Runtime.CompilerServices;

namespace ReactiveVars;

public static class ReactiveVarsLogger
{
	public static void Write(string s) => Console.Write(s);
	public static void WriteLine(string s) => Console.WriteLine(s);
	public static void WriteLine() => Console.WriteLine();

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
		var wrapD = MkD();
		d.D(wrapD);
		Disposable.Create(() => WriteLine($"[{name}].Dispose()")).D(wrapD);
		return wrapD;
	}
}