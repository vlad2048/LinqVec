﻿global using LanguageExt;
global using LanguageExt.Common;
global using static LanguageExt.Prelude;
global using Unit = LanguageExt.Unit;

global using Obs = System.Reactive.Linq.Observable;
global using Disp = System.Reactive.Disposables.CompositeDisposable;
global using static ReactiveVars.DispMaker;

global using static LogLib.Utils.CommonMakers;
global using L = LogLib.Logger;
global using LR = ReactiveVars.ReactiveVarsLogger;
global using LV = LinqVec.Utils.LinqVecLogger;

using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using LinqVec.Utils.Json;
using ReactiveUI;
using ReactiveVars;
using LogLib.Writers;
using LinqVec.Logging;


[assembly:InternalsVisibleTo("LINQPadQuery")]
[assembly:InternalsVisibleTo("LinqVec.Tests")]
[assembly:InternalsVisibleTo("Storybook")]

namespace LinqVec;

public static class Reseter
{
	public static void Reset()
	{
		ReactiveVars.Reseter.Reset();
	}
}

public static class G
{
	private const string ConfigFile = @"config\config.json";

	private static readonly Disp D = new();
	private static readonly IRwVar<IRoVar<Cfg>> CfgVar = Var.Make(RxCfg.Make(ConfigFile, default(Cfg), VecJsoner.Config), D);

	static G()
	{
		var schedD = new ScheduledDisposable(RxApp.TaskpoolScheduler, D);
		AppDomain.CurrentDomain.ProcessExit += (_, _) =>
		{
			schedD.Dispose();
		};
	}

	public static IRoVar<Cfg> Cfg => CfgVar.Switch().ToVar();
	//public static void OverrideCfg(Cfg cfg) => CfgVar.V = Var.MakeConst(cfg);
}


public static class CfgExt
{
	public static IRoVar<T> When<T>(this IObservable<Cfg> cfg, Func<Cfg, T> fun) => cfg.Select(fun).DistinctUntilChanged().ToVar();

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