using System.Reactive.Linq;
using Jot;
using ReactiveVars;
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

	public static void InitRX<T>(this Control ctrl, IObservable<T> whenInit, Action<T, Disp> initAction)
	{
		var d = MkD($"InitRX({ctrl.GetType().Name})").D(ctrl);
		ctrl.Events().HandleCreated.Subscribe(_ =>
		{
			whenInit.Subscribe(init => { initAction(init, d); }).D(d);
		}).D(d);
	}

	public static void InitRX(this Control ctrl, Action<Disp> initAction)
    {
        var d = MkD($"InitRX({ctrl.GetType().Name})").D(ctrl);
        ctrl.Events().HandleCreated.Subscribe(_ => initAction(d)).D(d);
    }

	public static Disp GetD(this Control ctrl) => MkD($"GetD({ctrl.GetType().Name})").D(ctrl);




    private static D D<D>(this D dispDst, Control ctrl) where D : IDisposable
    {
        ctrl.Events().HandleDestroyed.Merge(
                ctrl.Events().Disposed
            )
            .Take(1)
            .Subscribe(_ => dispDst.Dispose());
        return dispDst;
    }

    /*public static IObservable<T> MakeHot<T>(this IObservable<T> obs, Control ctrl)
    {
	    var con = obs.Publish();
	    con.Connect().D(ctrl);
	    return con;
    }*/
}