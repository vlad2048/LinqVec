using Microsoft.Reactive.Testing;
using System.Text;
using ReactiveVars;
using Shouldly;

namespace LinqVec.Tests.ModelTesting.TestSupport;

class ModelTestBase
{
	protected Disp D = null!;
	protected TestScheduler Sched = null!;
	protected Model<Doc> Model = null!;
	protected IPtr<Doc, Curve> Curve = null!;

	[SetUp]
	public void Setup()
	{
		ResetEvtGen();
		D = MkD();
		Sched = new TestScheduler();
		Model = new Model<Doc>(Doc.Empty(), D);
		Curve = Model.CurveCreate(D);
	}

	[TearDown]
	public void TearDown()
	{
		D.Dispose();
		var isIssue = LogAndTellIfThereAreUndisposedDisps(false, true);
		isIssue.ShouldBeFalse();
	}

	protected void SetT(double t)
	{
		Log();
		Sched.AdvanceTo(t.Sec());
	}

	protected void Log()
	{
		var sb = new StringBuilder();
		sb.Append($"[{TimeSpan.FromTicks(Sched.Clock).TotalSeconds:F1}s]".PadRight(7));
		sb.Append($"doc:{Model.Cur.V}".PadRight(20));
		sb.Append($"ptr:{Curve.V}".PadRight(20));
		sb.Append($"ptr-gfx:{Curve.ModGet()}".PadRight(20));
		L(sb.ToString());
	}

	protected ITestableObserver<T> Subs<T>(IObservable<T> when)
	{
		var obs = Sched.CreateObserver<T>();
		when.Subscribe(obs).D(D);
		return obs;
	}
}