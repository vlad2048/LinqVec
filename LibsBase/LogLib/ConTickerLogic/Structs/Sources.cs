using ReactiveVars;

namespace LogLib.ConTickerLogic.Structs;

public interface ISrc
{
	IObservable<Txt> WhenEvt { get; }
}

public sealed record VarSrc(IRoVar<Txt> Source) : ISrc
{
	public IObservable<Txt> WhenEvt => Obs.Never<Txt>();
}

public sealed record EvtSrc(IObservable<Txt> Source) : ISrc
{
	public IObservable<Txt> WhenEvt => Source;
}
