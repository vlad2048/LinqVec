using PowMaybe;

namespace LinqVec.Tools.Acts.Events;

interface ISeqEvt;
sealed record HoverActionSeqEvt(Maybe<Cursor> Cursor, Action Action) : ISeqEvt;
sealed record TriggerActionSeqEvt(Action Action) : ISeqEvt;
sealed record StartSeqEvt : ISeqEvt;
sealed record FinishSeqEvt(object Hotspot, Action Action) : ISeqEvt;
