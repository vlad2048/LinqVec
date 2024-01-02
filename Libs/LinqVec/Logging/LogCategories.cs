using LinqVec.Tools.Cmds.Events;
using LinqVec.Tools.Cmds.Logic;
using LinqVec.Tools.Cmds.Structs;
using LinqVec.Tools.Events;
using LogLib.Writers;
using LogLib;
using ReactiveVars;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace LinqVec.Logging;

static class LogCategories
{
	// Time
	// ====
	public static void Setup_Time_Logging(IScheduler scheduler, Disp d) =>
		LogCategoriesExt.RegisterPrefix(
			() => new TimestampCon(scheduler.Now),
			"Time",
			_ => true,
			d
		);

	// Hotspot
	// =======
	public static void Setup_Hotspot_Logging(IRoVar<bool> isHotspotFrozen, IRoVar<Option<Hotspot>> hotspot, IScheduler scheduler, Disp d)
	{
		// IsHotspotFrozen
		// ---------------
		isHotspotFrozen.Select(e => new IsHotspotFrozenCon(e)).ToVar()
			.RegisterPrefix(
				"frozen",
				cfg => cfg.Log.LogCmd.Hotspot,
				d
			);


		// Hotspot?.HotspotNfo.Name
		// ------------------------
		hotspot
			.Where(_ => G.Cfg.V.Log.LogCmd.Hotspot)
			.Select(e => new HotspotNameCon(e.Map(f => f.HotspotNfo.Name).IfNone("_")))
			.Write(d);
	}


	// Evt
	// ***
	public static void Setup_Evt_Logging(IObservable<IEvt> evt, IScheduler scheduler, Disp d) =>
		evt
			.Where(_ => G.Cfg.V.Log.LogCmd.Evt)
			.Write(d);


	// UsrEvt
	// ******
	public static void Setup_UsrEvt_Logging(IObservable<IUsr> evt, IScheduler scheduler, Disp d) =>
		evt
			.Where(_ => G.Cfg.V.Log.LogCmd.UsrEvt)
			.Write(d);

	// CmdEvt
	// ******
	public static void Setup_CmdEvt_Logging(IObservable<ICmdEvt> evt, IScheduler scheduler, Disp d) =>
		evt
			.Where(_ => G.Cfg.V.Log.LogCmd.CmdEvt)
			.Write(d);
}





file static class LogCategoriesExt
{
	public static void Write<T>(this IObservable<T> source, Disp d) where T : IWriteSer =>
		source.Subscribe(src => LogVecConKeeper.Instance.Gen(src)).D(d);

	public static void RegisterPrefix<T>(
		this IObservable<T> sourceObs,
		string name,
		Func<Cfg, bool> enableFun,
		Disp d
	) where T : IWriteSer
	{
		var source = sourceObs.ToVar();
		LogVecConKeeper.Instance.RegisterPrefix(name, Mk(() => source.V).EnableWhen(() => enableFun(G.Cfg.V))).D(d);
	}

	public static void RegisterPrefix<T>(
		Func<T> valFun,
		string name,
		Func<Cfg, bool> enableFun,
		Disp d
	) where T : IWrite =>
		LogVecConKeeper.Instance.RegisterPrefix(name, valFun.EnableWhen(() => enableFun(G.Cfg.V))).D(d);

	private static Func<Option<T>> EnableWhen<T>(this Func<T> funPrev, Func<bool> enable) =>
		() => enable() switch {
			false => None,
			true => Some(funPrev())
		};
	
	private static Func<T> Mk<T>(this Func<T> f) => f;
}