using LinqVec.Tests.ModelTesting.TestSupport;
using Microsoft.Reactive.Testing;
using PowBasics.CollectionsExt;
using Shouldly;
using P = (int, int);

namespace LinqVec.Tests.Tools.Cmds.TestSupport;

static class DocChecker
{
	public static void AssertEq(this ITestableObserver<Doc> obs, (double, P[][])[] curvesExp)
	{
		var curvesAct = obs.Messages.SelectToArray(e => e.Value.Value.Simplify(TimeSpan.FromTicks(e.Time).TotalSeconds));
		curvesAct.Length.ShouldBe(curvesExp.Length);
		Check(curvesAct, curvesExp).ShouldBeTrue();
	}

	private static bool Check((double, P[][])[] act, (double, P[][])[] exp) => act.Length == exp.Length && act.Zip(exp).All(t => Check(t.Item1, t.Item2));
	private static bool Check((double, P[][]) act, (double, P[][]) exp) => act.Item1 == exp.Item1 &&  act.Item2.Length == exp.Item2.Length && act.Item2.Zip(exp.Item2).All(t => Check(t.Item1, t.Item2));
	private static bool Check(P[] act, P[] exp) => act.Length == exp.Length && act.Zip(exp).All(t => Check(t.Item1, t.Item2));
	private static bool Check(P act, P exp) => act == exp;

	private static (double, P[][]) Simplify(this Doc doc, double t) =>
		(
			t,
			doc.Layers[0].Objects.OfType<Curve>().SelectToArray(
				e => e.Pts.SelectToArray(f => (f.Start, f.End))
			)
		);
}