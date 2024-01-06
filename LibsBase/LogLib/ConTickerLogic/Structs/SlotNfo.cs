namespace LogLib.ConTickerLogic.Structs;


public enum SlotType
{
	Var,
	Event
}

public sealed record SlotNfo(
	SlotType Type,
	string Name,
	int Priority,
	int Size,
	IObservable<bool> WhenEnabled
);

sealed record SlotUnsortedInst(
	SlotNfo Nfo,
	ISrc Source
);

sealed record SlotInst(
	SlotLoc Loc,
	SlotNfo Nfo,
	ISrc Source
);
