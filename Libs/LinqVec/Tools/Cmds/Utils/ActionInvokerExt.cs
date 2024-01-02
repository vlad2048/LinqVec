/*
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveVars;

namespace LinqVec.Tools.Cmds.Utils;

static class ActionInvokerExt
{
	public static void RunFuncAction(this IObservable<Func<Action>> source, DISP d) =>
		source
			.Select(e => e.ToFuncDisp()())
			.DisposePrevious()
			.MakeHot(d);

	private static Func<IDisposable> ToFuncDisp(this Func<Action> fun) => () => Disposable.Create(fun());
}
*/
