using PowMaybe;

namespace LinqVec.Tools.Acts.Events;

interface IHotEvt;
sealed record OverHotEvt(Maybe<object> MHotspot) : IHotEvt;
sealed record TriggerHotEvt(object Hotspot) : IHotEvt;
