using System.Reactive.Concurrency;
using System.Reactive.Linq;
using LinqVec.Logic;
using LinqVec.Tools.Cmds.Logic;
using LinqVec.Utils.Rx;

namespace LinqVec.Tools.Cmds.Events;

// @formatter:off
public interface IDbgEvt;
public sealed record RunDbgEvt(IRunEvt Evt) : IDbgEvt { public override string ToString() => $"Run({Evt})"; }
//public sealed record ModDbgEvt(IModEvt Evt) : IDbgEvt { public override string ToString() => $"Mod({Evt})"; }
public sealed record CmdDbgEvt(ICmdEvt Evt) : IDbgEvt { public override string ToString() => $"Cmd({Evt})"; }
// @formatter:on


public static class DbgEvtUtils
{
	public static IObservable<IDbgEvt> Make<O>(CmdOutput cmdOutput, Unmod<O> ptr, IScheduler scheduler) =>
		Obs.Merge<IDbgEvt>(
				//ptr.WhenModEvt.Select(e => new ModDbgEvt(e)),
				cmdOutput.WhenCmdEvt.Select(e => new CmdDbgEvt(e)),
				cmdOutput.WhenRunEvt.Select(e => new RunDbgEvt(e))
			)
			.OrderLogs(scheduler, e => e is RunDbgEvt, e => e is CmdDbgEvt); //, e => e is ModDbgEvt);
}