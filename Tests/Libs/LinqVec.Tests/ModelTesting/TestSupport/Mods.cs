using System.Reactive.Linq;
using Geom;
using LinqVec.Logic.Structs;
using LinqVec.Utils;
using ReactiveVars;

namespace LinqVec.Tests.ModelTesting.TestSupport;

static class Mods
{
	public static Mod<Curve> AddPoint_Hover(IRoVar<Option<Pt>> mouse, Disp d) =>
		new(
			nameof(AddPoint_Hover),
			false,
			mouse
				.WhereSome()
				.Select(m => Mk(curve =>
					curve with
					{
						Pts = curve.Pts.AddArr(new CurvePt((int)m.X, (int)m.X))
					}))
				.ToVar(d)
		);

	public static Func<Pt, Mod<Curve>> AddPoint_Drag(IRoVar<Option<Pt>> mouse, Disp d) =>
		startPt =>
			new(
				nameof(AddPoint_Drag),
				true,
				mouse
					.WhereSome()
					.Select(m => Mk(curve =>
						curve with
						{
							Pts = curve.Pts.AddArr(new CurvePt((int)startPt.X, (int)m.X))
						}))
					.ToVar(d)
			);

	private static Func<Curve, Curve> Mk(Func<Curve, Curve> f) => f;
}