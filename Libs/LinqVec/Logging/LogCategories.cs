using LinqVec.Tools.Cmds.Events;
using LinqVec.Tools.Cmds.Logic;
using LinqVec.Tools.Cmds.Structs;
using LinqVec.Tools.Events;
using LogLib.Writers;
using LogLib;
using ReactiveVars;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using LogLib.Interfaces;
using PtrLib;

namespace LinqVec.Logging;

public static class LogCategories
{
	// Time
	// ====
	internal static void Setup_Time_Logging(IScheduler scheduler, Disp d)
	{
		var lastTime = scheduler.Now;
		LogCategoriesExt.RegisterPrefix(
			() =>
			{
				var now = scheduler.Now;
				var delta = now - lastTime;
				lastTime = now;
				return new TimestampCon(delta);
			},
			"Time",
			_ => true,
			d
		);
	}

	// Hotspot
	// =======
	internal static void Setup_Hotspot_Logging(IRoVar<bool> isDragging, IRoVar<Option<Hotspot>> hotspot, IScheduler scheduler, Disp d)
	{
		// IsDragging
		// ----------
		isDragging.Select(e => new IsDraggingCon(e)).ToVar()
			.RegisterPrefix(
				"dragging",
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
	internal static void Setup_Evt_Logging(IObservable<IEvt> evt, IScheduler scheduler, Disp d) =>
		evt
			.Where(_ => G.Cfg.V.Log.LogCmd.Evt)
			.Write(d);


	// UsrEvt
	// ******
	internal static void Setup_UsrEvt_Logging(IObservable<IUsr> evt, IScheduler scheduler, Disp d) =>
		evt
			.Where(_ => G.Cfg.V.Log.LogCmd.UsrEvt)
			.Write(d);

	// CmdEvt
	// ******
	internal static void Setup_CmdEvt_Logging(IObservable<ICmdEvt> evt, IScheduler scheduler, Disp d) =>
		evt
			.Where(_ => G.Cfg.V.Log.LogCmd.CmdEvt)
			.Write(d);

	// ModEvt
	// ******
	public static void Setup_ModEvt_Logging(IObservable<IModEvtF> evt, IScheduler scheduler, Disp d) =>
		evt
			.Where(_ => G.Cfg.V.Log.LogCmd.ModEvt)
			.Write(d);


}
public interface IModEvtF : IWriteSer;
public sealed record ModStartEvtF(string Name) : IModEvtF
{
	public override string ToString() => $"Start({Name})";
	public ITxtWriter Write(ITxtWriter w) => this.Color(w);
}
public sealed record ModFinishEvtF(string Name, bool Commit, string Str) : IModEvtF
{
	public override string ToString() => $"{Verb}({Name})  -> {Str}";
	private string Verb => Commit ? "Commit" : "Cancel";
	public ITxtWriter Write(ITxtWriter w) => this.Color(w);
}

public static class ModEvtExt
{
	public static IModEvtF Conv(this IModEvt e) => e switch {
		ModStartEvt f => new ModStartEvtF(f.Name),
		ModFinishEvt f => new ModFinishEvtF(f.Name, f.Commit, f.Str),
		_ => throw new ArgumentException()
	};
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