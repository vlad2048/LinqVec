namespace LinqVec.Tools.Acts.Events;

interface IHotEvt;
sealed record OverHotEvt(Option<object> MHotspot) : IHotEvt;
sealed record TriggerHotEvt(object Hotspot) : IHotEvt;
