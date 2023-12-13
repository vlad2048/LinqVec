/*
using System.Reactive.Disposables;

namespace SmartReactives;

public static class DispExt
{
	public static T D<T>(this (T, IDisposable) t, IDisposeNotification d)
	{
		d.WhenDisposed.Subscribe(_ => t.Item2.Dispose());
		return t.Item1;
	}

	public static Dictionary<K, V> D<K, V>(this Dictionary<K, V> dict, IDisposeNotification d)
		where K : notnull
		where V : IDisposable
	{
		Disposable.Create(() =>
		{
			foreach (var val in dict.Values)
				val.Dispose();
			dict.Clear();
		}).D(d);
		return dict;
	}


	public static IDisposable SubscribeWithDisp<T>(
		this IObservable<T> obs,
		Action<T, IDisposeNotification> action
	)
	{
		var d = new Disp();
		var serD = new SerialDisposable().D(d);
		obs.Subscribe(val =>
		{
			serD.Disposable = null;
			if (val == null) return;

			var innerD = new Disp();
			action(val, innerD);
			serD.Disposable = innerD;
		}).D(d);
		return d;
	}

	public static T D<T>(this T obj, IDisposeNotification d) where T : IDisposable
	{
		d.WhenDisposed.Subscribe(_ => obj.Dispose());
		return obj;
	}
}
*/