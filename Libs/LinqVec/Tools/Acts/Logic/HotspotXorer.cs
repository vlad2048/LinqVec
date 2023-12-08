using LinqVec.Tools.Acts.Structs;
using LinqVec.Utils;
using PowBasics.CollectionsExt;
using PowMaybe;

namespace LinqVec.Tools.Acts.Logic;

static class HotspotXorer
{
	public static IActExpr XorHotspots(this IActExpr root) =>
		root
			.MapAmbs(XorAmbHotspots);


	private static IActExpr MapAmbs(this IActExpr root, Func<AmbActExpr, AmbActExpr> fun)
	{
		IActExpr Rec(IActExpr node) => node switch
		{
			BaseActExpr => node,
			SeqActExpr { Acts: var acts } => new SeqActExpr(acts.SelectToArray(Rec)),
			LoopActExpr { Act: var act } => new LoopActExpr(Rec(act)),
			AmbActExpr e => fun(e),
			_ => throw new ArgumentException()
		};

		return Rec(root);
	}


	private static AmbActExpr XorAmbHotspots(AmbActExpr amb)
	{
		var kids = GetAmbBaseKids(amb);
		var hotspots = kids.SelectToArray(e => e.Act.Hotspot);
		var hotspotsNext = new Func<Pt, Maybe<object>>[hotspots.Length];
		for (var i = 0; i < hotspots.Length; i++)
		{
			var capI = i;
			hotspotsNext[i] = m =>
			{
				for (var j = 0; j < capI; j++)
					if (hotspots[j](m).IsSome())
						return May.None<object>();
				return hotspots[capI](m);
			};
		}
		var kidsNext = kids.SelectToArray((e, i) => new BaseActExpr(e.Act with { Hotspot = hotspotsNext[i] }));
		var ambNext = SetAmbBaseKids(amb, kidsNext);
		return ambNext;
	}

	private static BaseActExpr[] GetAmbBaseKids(AmbActExpr amb) => amb.Acts.SelectToArray(GetFirstBaseKid);
	private static AmbActExpr SetAmbBaseKids(AmbActExpr amb, BaseActExpr[] kids) => new(amb.Acts.SelectToArray((e, i) => SetFirstBaseKid(e, kids[i])));

	private static BaseActExpr GetFirstBaseKid(IActExpr root) =>
		root switch
		{
			BaseActExpr e => e,
			SeqActExpr { Acts: var acts } => GetFirstBaseKid(acts[0]),
			AmbActExpr { Acts: var acts } => GetFirstBaseKid(acts[0]),
			LoopActExpr { Act: var act } => GetFirstBaseKid(act),
			_ => throw new ArgumentException()
		};

	private static IActExpr SetFirstBaseKid(IActExpr root, BaseActExpr baseKid) =>
		root switch
		{
			BaseActExpr => baseKid,
			SeqActExpr { Acts: var acts } => new SeqActExpr(acts.SetIdx(0, SetFirstBaseKid(acts[0], baseKid))),
			AmbActExpr { Acts: var acts } => new AmbActExpr(acts.SetIdx(0, SetFirstBaseKid(acts[0], baseKid))),
			LoopActExpr { Act: var act } => new LoopActExpr(SetFirstBaseKid(act, baseKid)),
			_ => throw new ArgumentException()
		};
	}