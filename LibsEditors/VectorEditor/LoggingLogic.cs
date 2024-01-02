using LinqVec;
using PtrLib;
using System.Reactive.Concurrency;
using ReactiveVars;
using VectorEditor._Model;

namespace VectorEditor;

static class LoggingLogic
{
	public static void Setup_CurveMod_Logging(IScopedPtr<Curve> curve, IScheduler scheduler, Disp d) =>
		G.Cfg.RunWhen(e => e.Log.LogCmd.ModEvt, d, [
			() => curve.WhenModEvt.LogD("ModEvt"),
		]);
}