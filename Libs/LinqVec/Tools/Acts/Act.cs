using System.Reactive.Disposables;
using System.Reactive.Linq;
using Geom;
using LinqVec.Logic;
using LinqVec.Tools.Acts.Enums;
using LinqVec.Tools.Acts.Events;
using LinqVec.Tools.Events;
using LinqVec.Utils;
using LinqVec.Utils.Rx;
using PowRxVar;
using PowRxVar.Utils;
using UILib;

namespace LinqVec.Tools.Acts;


//public delegate Obj Mod<Obj, in Hot>(Obj obj, Hot hot, Pt ptStart, Pt ptEnd);
//public delegate Func<Obj, Pt, Obj> Mod<Obj, in Hot>(Hot hot, Option<Pt> ptStart);

//public delegate Obj Mod<Obj, in Hot>(Obj obj, Hot hot, Pt mouse);

public delegate Func<Obj, Pt, Obj> Mod<Obj>(Option<Pt> ptStart);
public delegate Func<Obj, Pt, Obj> Mod<Obj, in Hot>(Hot hot);


public sealed record Actions(
	Action<Option<object>>? OnHover,
	Action<(Pt ptDragStart, object hot)>? OnDragStart,
	//Action<(Pt ptDragStart, Pt ptDragEnd, object hot)>? OnDragEnd,
	Action<Pt>? OnDragEnd,
	Action<object>? OnClick
)
{
	public static readonly Actions Empty = new(null, null, null, null);

	public static Actions Click<Hot>(Action<Hot> action) => new(
		OnHover: null,
		OnDragStart: null,
		OnDragEnd: null,
		OnClick: obj => action((Hot)obj)
	);

	public static Actions<Hot> Drag<Obj, Hot>(IModder<Obj> modder, Mod<Obj> mod, Action setStateHover, Action setStateStart) => new(
		OnHover: hotOpt => hotOpt.Match(
			hot =>
			{
				//L.WriteLine("[Drag].Hover(true) -> ModSet");
				setStateHover();
				modder.ModSet(mod(None));
			},
			() =>
			{
				//L.WriteLine("[Drag].Hover(false) -> ModClear");
				modder.ModClear();
			}),
		OnDragStart: t =>
		{
			//L.WriteLine($"[Drag].DragStart({t.ptDragStart})");
			setStateStart();
			modder.ModSet(mod(t.ptDragStart));
		},
		OnDragEnd: pt =>
		{
			//L.WriteLine($"[Drag].DragEnd({pt}) -> ModApply");
			modder.ModApply(pt);
		},
		OnClick: null
	);

	public static Actions<Hot> Drag<Obj, Hot>(IModder<Obj> modder, Mod<Obj, Hot> mod, Action setStateHover, Action setStateStart) => new(
		OnHover: hotOpt => hotOpt.Match(
			hot =>
			{
				setStateHover();
				modder.ModSet(mod(hot));
			},
			modder.ModClear
		),
		OnDragStart: t =>
		{
			setStateStart();
			modder.ModSet(mod(t.hot));
		},
		OnDragEnd: modder.ModApply,
		OnClick: null
	);
}

public sealed record Actions<H>(
	Action<Option<H>>? OnHover,
	Action<(Pt ptDragStart, H hot)>? OnDragStart,
	Action<Pt>? OnDragEnd,
	Action<H>? OnClick
)
{
	public static readonly Actions<H> Empty = new(null, null, null, null);
}

public static class ActionsExt
{
	public static Actions ToActions<T>(this Actions<T> a) => new(
		OnHover: hOpt => a.OnHover?.Invoke(hOpt.Map(e => (T)e)),
		OnDragStart: t => a.OnDragStart?.Invoke((t.ptDragStart, (T)t.hot)),
		//OnDragEnd: t => a.OnDragEnd?.Invoke((t.ptDragStart, t.ptDragEnd, (T)t.hot)),
		OnDragEnd: a.OnDragEnd,
		OnClick: h => a.OnClick?.Invoke((T)h)
	);
}




public sealed record Act(
	string Name,
	Func<Pt, Option<object>> Hotspot,
	Gesture Gesture,
	Cursor? Cursor,
	Actions Actions
)
{
	public static Act Make<H>(
		string name,
		Func<Pt, Option<H>> hotspot,
		Gesture gesture,
		Cursor? cursor,
		Actions<H> actions
	) => new(
		name,
		p => hotspot(p).Map(e => (object)e),
		gesture,
		cursor,
		actions.ToActions()
	);
}


public static class ActRunner
{
	private sealed record Hot(object H, Act Act);

	public static IDisposable Run(
		Evt evt,
		IObservable<Unit> whenUndoRedo,
		Act[] acts
	)
	{
		var d = new Disp();

		var gestures = acts.Select(e => e.Gesture).Aggregate(Gesture.None, (a, e) => a | e);
		var actEvt = evt.ToActEvt(gestures, d);
		var (setState, whenState) = RxEventMaker.Make<Act[]>().D(d);
		void Reset() => setState(acts);

		//d.Log("ActRunnerDisp");

		whenUndoRedo.Subscribe(_ =>
		{
			//L.WriteLine("Reset()");
			Reset();
		}).D(d);

		whenState.SubscribeWithDisp((acts_, stateD) =>
		{
			var hotLocked = false;
			var hot = evt.TrackHotspot(acts_, () => hotLocked).D(stateD);
			//hot.Select(e => e.Map(f => f.Act.Name)).Log(stateD, "Hot");
			actEvt
				.Subscribe(e =>
				{
					hot.V.IfSome(hotV =>
					{
						switch (e)
						{
							case DragStartActEvt { PtStart: var ptStart } when hotV.Act.Gesture == Gesture.Drag:
								hotLocked = true;
								hotV.Act.Actions.OnDragStart?.Invoke((ptStart, hotV.H));
								break;
							case DragEndActEvt { PtStart: var ptStart, PtEnd: var ptEnd } when hotV.Act.Gesture == Gesture.Drag:
								hotLocked = false;
								hotV.Act.Actions.OnDragEnd?.Invoke(ptEnd);
								Reset();
								break;

							case ClickActEvt { Pt: var pt } when hotV.Act.Gesture == Gesture.Click:
								hotV.Act.Actions.OnClick?.Invoke(hotV.H);
								Reset();
								break;
							case RightClickActEvt { Pt: var pt } when hotV.Act.Gesture == Gesture.RightClick:
								hotV.Act.Actions.OnClick?.Invoke(hotV.H);
								Reset();
								break;
							case DoubleClickActEvt { Pt: var pt } when hotV.Act.Gesture == Gesture.DoubleClick:
								hotV.Act.Actions.OnClick?.Invoke(hotV.H);
								Reset();
								break;
						}

					});
				}).D(stateD);
		}).D(d);

		Reset();


		return d;
	}

	private static (IRoVar<Option<Hot>>, IDisposable) TrackHotspot(this Evt evt, Act[] acts, Func<bool> hotLocked)
	{
		var d = new Disp();
		var hotPrev = Option<Act>.None;
		var hot = Var.Make(Option<Hot>.None).D(d);

		evt.MousePos
			.Subscribe(mouseOpt => mouseOpt.Match(
				mouse =>
				{
					foreach (var act in acts)
					{
						var mayH = act.Hotspot(mouse);
						if (mayH.IsSome)
						{
							var isAlreadySet = hot.V.Map(e => e.Act == act).IfNone(false);
							if (!isAlreadySet && !hotLocked())
								hot.V = Some(new Hot(mayH.IfNone(() => throw new ArgumentException()), act));
							return;
						}
					}
					if (!hotLocked())
						hot.V = None;
				},
				() =>
				{
					if (!hotLocked())
						hot.V = None;
				})).D(d);

		hot
			.ObserveOnUI()
			.Subscribe(h =>
			{
				hotPrev.IfSome(a =>
				{
					if (h.IsNone)
						evt.SetCursor(Cursors.Default);
					a.Actions.OnHover?.Invoke(Option<object>.None);
				});
				hotPrev = h.Map(e => e.Act);
				h.IfSome(a =>
				{
					evt.SetCursor(a.Act.Cursor ?? Cursors.Default);
					a.Act.Actions.OnHover?.Invoke(Some(a.H));
				});
			}).D(d);

		return (hot.ToReadOnly(), d);
	}
}