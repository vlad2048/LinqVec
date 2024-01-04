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
	int Size
);
