using System.Reactive.Linq;
using Jot;
using PowRxVar;
using UILib.Utils;

namespace UILib;

public static class WinFormsUtils
{
	public static Tracker Tracker { get; } = new();

	static WinFormsUtils()
	{
		TrackerSetup.Init(Tracker);
	}

	public static T Track<T>(this T obj) where T : class
	{
		switch (obj)
		{
			case Form form:
				form.Events().Load.Subscribe(_ => Tracker.Track(obj)).D(form);
				break;

			default:
				Tracker.Track(obj);
				break;
		}

		return obj;
	}

	public static void InitRX<T>(this Control ctrl, IObservable<T> whenInit, Action<T, IRoDispBase> initAction)
	{
		var d = new Disp().D(ctrl);
		ctrl.Events().HandleCreated.Subscribe(_ =>
		{
            whenInit.Subscribe(init =>
            {
	            initAction(init, d);
            }).D(d);
		}).D(d);
	}

	public static void InitRX(this Control ctrl, Action<IRoDispBase> initAction)
    {
        var d = new Disp().D(ctrl);
        ctrl.Events().HandleCreated.Subscribe(_ => initAction(d)).D(d);
    }

    public static D D<D>(this D dispDst, Control ctrl) where D : IDisposable
    {
        ctrl.Events().HandleDestroyed.Merge(
                ctrl.Events().Disposed
            )
            .Take(1)
            .Subscribe(_ => dispDst.Dispose());
        return dispDst;
    }

    public static T D<T>(this (T, IDisposable) t, Control ctrl)
    {
	    t.Item2.D(ctrl);
	    return t.Item1;
	}

    public static (T1, T2) D<T1, T2>(this (T1, T2, IDisposable) t, Control ctrl)
    {
	    t.Item3.D(ctrl);
	    return (t.Item1, t.Item2);
    }

    public static IObservable<T> MakeHot<T>(this IObservable<T> obs, Control ctrl)
    {
	    var con = obs.Publish();
	    con.Connect().D(ctrl);
	    return con;
    }

	public static IObservable<T> ObserveOnUI<T>(this IObservable<T> obs)
	{
        //L.WriteLine($"ThreadCtx: {SynchronizationContext.Current != null}");
        //if (SynchronizationContext.Current == null) return obs;
		return obs.ObserveOn(SynchronizationContext.Current!);
	}
}