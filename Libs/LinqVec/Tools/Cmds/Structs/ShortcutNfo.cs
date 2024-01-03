using System.Text.Json.Serialization;

namespace LinqVec.Tools.Cmds.Structs;

public sealed record ShortcutNfo(
	string Name,
	Keys Key,
	[property: JsonIgnore]
	Action Action
);