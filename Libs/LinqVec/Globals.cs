global using LanguageExt;
global using LanguageExt.Common;
global using static LanguageExt.Prelude;
global using Unit = LanguageExt.Unit;

global using Obs = System.Reactive.Linq.Observable;
global using Disp = System.Reactive.Disposables.CompositeDisposable;
global using static PowRxVar.DispMaker;

global using static LinqVec.Utils.CommonMakers;
global using L = LinqVec.Utils.Logger;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using LinqVec.Utils.Json;
using PowRxVar;
using ReactiveUI;

[assembly:InternalsVisibleTo("LINQPadQuery")]
[assembly:InternalsVisibleTo("LinqVec.Tests")]

namespace LinqVec;

static class G
{
	private const string ConfigFile = @"config\config.json";

	private static readonly Disp D = new();

	static G()
	{
		var schedD = new ScheduledDisposable(RxApp.TaskpoolScheduler, D);
		AppDomain.CurrentDomain.ProcessExit += (_, _) =>
		{
			schedD.Dispose();
		};
	}

	public static IObservable<Cfg> Cfg { get; } = RxCfg.Make(ConfigFile, default(Cfg), VecJsoner.Default, D);
}


static class CfgExt
{
	public static IObservable<T> When<T>(this IObservable<Cfg> cfg, Func<Cfg, T> fun) => cfg.Select(fun).DistinctUntilChanged();

	public static IDisposable RunWhen(this IObservable<Cfg> cfg, Func<Cfg, bool> fun, Func<IDisposable> action)
	{
		var d = new Disp();
		var serDisp = new SerDisp().D(d);

		cfg.When(fun)
			.Subscribe(on =>
			{
				var serD = serDisp.GetNewD();
				if (on)
					action().D(serD);
			}).D(d);

		return d;
	}
}