/*
using System.Reactive.Linq;
using Geom;
using LinqVec.Tests.ModelTesting.TestSupport;
using LinqVec.Utils;
using Microsoft.Reactive.Testing;
using ReactiveVars;
using TestLib;

namespace LinqVec.Tests.ModelTesting;

class PtrModTests : ModelTestBase
{
    [Test]
    public void _00_SetApply()
    {
        var paintObs = Sched.CreateObserver<Unit>();
        Model.WhenPaintNeeded.Subscribe(paintObs);

        SetT(1);
        Curve.ModSet(new Mod<Curve>("Test", true, Var.MakeConst<Func<Curve, Curve>>(curve_ => curve_ with { Pts = curve_.Pts.AddArr(new CurvePt(7, 7)) })));

		SetT(2);
		Curve.ModSet(Mod<Curve>.Empty);

		SetT(3);
        
		paintObs.LogMessages("model.WhenPaintNeeded");
    }

    [Test]
    public void _01_SetMouseApply()
    {
	    var mouse = new Pt(120, 20).Make(D);
	    var paintObs = Sched.CreateObserver<Unit>();
	    Model.WhenPaintNeeded.Subscribe(paintObs);

	    var ptrObs = Sched.CreateObserver<Unit>();
	    Curve.WhenPaintNeeded.Subscribe(ptrObs);

		SetT(1);
	    Curve.ModSet(
			new Mod<Curve>(
				"Test",
				true,
				mouse
					.Select(m => Mk(curve_ => curve_ with { Pts = curve_.Pts.AddArr(new CurvePt((int)m.X, (int)m.X)) }))
					.ToVar()
			)
		);

	    SetT(2);

	    SetT(3);
	    mouse.V = new Pt(130, 20);

	    SetT(4);
	    Curve.ModSet(Mod<Curve>.Empty);

		SetT(5);
	    mouse.V = new Pt(140, 20);

	    SetT(6);
	    mouse.V = new Pt(150, 20);

	    SetT(7);

		ptrObs.LogMessages("Ptr.WhenPaintNeeded");
	    paintObs.LogMessages("Model.WhenPaintNeeded");

		paintObs.Messages.AssertEqual([
			OnNext(0, Unit.Default),
			OnNext(1, Unit.Default),
			OnNext(3, Unit.Default),
			OnNext(4, Unit.Default),
		]);
    }

	private static Func<Curve, Curve> Mk(Func<Curve, Curve> f) => f;
}
*/




