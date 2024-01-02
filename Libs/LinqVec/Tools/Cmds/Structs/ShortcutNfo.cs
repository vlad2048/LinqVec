namespace LinqVec.Tools.Cmds.Structs;

public sealed record ShortcutNfo(
	string Name,
	Keys Key,
	Action Action
);