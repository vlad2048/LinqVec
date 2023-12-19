global using LanguageExt;
global using LanguageExt.Common;
global using static LanguageExt.Prelude;
global using Unit = LanguageExt.Unit;

global using Obs = System.Reactive.Linq.Observable;
global using Disp = System.Reactive.Disposables.CompositeDisposable;
global using static ReactiveVars.DispMaker;

global using static LinqVec.Utils.CommonMakers;
global using L = LinqVec.Utils.Logger;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using LinqVec.Utils.Json;
using ReactiveUI;
using ReactiveVars;

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

	public static IObservable<Cfg> Cfg { get; } = RxCfg.Make(ConfigFile, default(Cfg), VecJsoner.Default);
}


static class CfgExt
{
	public static IObservable<T> When<T>(this IObservable<Cfg> cfg, Func<Cfg, T> fun) => cfg.Select(fun).DistinctUntilChanged();

	public static void RunWhen(this IObservable<Cfg> cfg, Func<Cfg, bool> fun, Disp d, params Func<IDisposable>[] actions)
	{
		var serDisp = new SerDisp().D(d);

		cfg.When(fun)
			.Subscribe(on =>
			{
				var serD = serDisp.GetNewD();
				if (on)
				{
					foreach (var action in actions)
						action().D(serD);
				}
			}).D(d);
	}
}