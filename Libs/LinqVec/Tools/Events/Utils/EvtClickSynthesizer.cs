using PowRxVar.Utils;
using PowRxVar;
using System.Reactive;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Geom;
using UILib;

namespace LinqVec.Tools.Events.Utils;

public static class EvtClickSynthesizer
{
	private interface ISynth;
	private sealed record NoneSynth : ISynth;
	private sealed record ClickSynth(MouseBtn Btn, Pt Pos) : ISynth;

	private static readonly TimeSpan ClickTime = TimeSpan.FromMilliseconds(500);

	public static IObservable<IEvt> SynthesizeClicks(this IObservable<IEvt> src, IRoDispBase d) =>
		Obs.Create<IEvt>(obs =>
		{
			var obsD = new Disp();
			void Send(IEvt evtDst) => obs.OnNext(evtDst);
			var state = Var.Make<ISynth>(new NoneSynth()).D(obsD);

			var (timeout, whenTimeout) = RxEventMaker.Make<Unit>().D(obsD);
			var timeD = new SerialDisp<IRwDispBase>().D(obsD);
			timeD.Value = new Disp();

			//void TimeoutSched() => Obs.Timer(ClickTime, new SynchronizationContextScheduler(SynchronizationContext.Current!)).Subscribe(_ => timeout(Unit.Default)).D(timeD.Value);

			void TimeoutSched() => Obs.Timer(ClickTime).Subscribe(_ => timeout(Unit.Default)).D(timeD.Value);

			void TimeoutCancel()
			{
				timeD.Value = null;
				timeD.Value = new Disp();
			}

			whenTimeout
				.ObserveOnUI()
				.Subscribe(_ =>
				{
					if (state.V is ClickSynth { Btn: var stateBtn, Pos: var statePos })
						Send(new MouseBtnEvt(statePos, UpDown.Down, stateBtn));
					TimeoutCancel();
					state.V = new NoneSynth();
				}).D(obsD);

			src.Subscribe(evtSrc =>
			{
				switch (state.V)
				{
					case NoneSynth:
						switch (evtSrc)
						{
							case MouseBtnEvt { UpDown: UpDown.Down, Btn: var btn, Pos: var pos }:
								state.V = new ClickSynth(btn, pos);
								TimeoutSched();
								break;
							default:
								Send(evtSrc);
								break;
						}
						break;

					case ClickSynth { Btn: var stateBtn, Pos: var statePos }:
						switch (evtSrc)
						{
							case MouseBtnEvt { UpDown: UpDown.Up, Btn: var btn, Pos: var pos } when btn == stateBtn:
								Send(new MouseClickEvt(statePos, stateBtn));
								break;
							default:
								Send(new MouseBtnEvt(statePos, UpDown.Down, stateBtn));
								Send(evtSrc);
								break;
						}
						TimeoutCancel();
						state.V = new NoneSynth();
						break;
				}
			}).D(obsD);

			return obsD;
		})
			.MakeHot(d);



	public static IObservable<IEvt> NoClicks(this IObservable<IEvt> src) =>
		src
			.Select(e => e switch
			{
				MouseClickEvt { Btn: var btn, Pos: var pos } => new IEvt[]
				{
					new MouseBtnEvt(pos, UpDown.Down, btn),
					new MouseBtnEvt(pos, UpDown.Up, btn),
				}.ToObservable(),

				/*MouseClickEvt { Btn: var btn, Pos: var pos } =>
					Obs.Return(new MouseBtnEvt(pos, UpDown.Down, btn)).Concat(
						//Obs.Return(new MouseBtnEvt(pos, UpDown.Up, btn)).Delay(TimeSpan.FromMilliseconds(100), new SynchronizationContextScheduler(SynchronizationContext.Current!))
						Obs.Return(new MouseBtnEvt(pos, UpDown.Up, btn)).Delay(TimeSpan.FromMilliseconds(100))
					),*/

				_ => Obs.Return(e)
			})
			.Merge();
}