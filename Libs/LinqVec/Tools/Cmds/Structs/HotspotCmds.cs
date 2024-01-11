using Geom;
using LinqVec.Tools.Cmds.Enums;
using ReactiveVars;
using System.Text.Json.Serialization;

namespace LinqVec.Tools.Cmds.Structs;


[JsonDerivedType(typeof(ClickHotspotCmd), typeDiscriminator: "ClickHotspotCmd")]
[JsonDerivedType(typeof(DragHotspotCmd), typeDiscriminator: "DragHotspotCmd")]
public interface IHotspotCmd
{
	string Name { get; }
}

public sealed record ClickHotspotCmd(
	string Name,
	Gesture Gesture,
	[property: JsonIgnore]
	Action ClickAction
) : IHotspotCmd;

public sealed record DragHotspotCmd(
	string Name,
	[property: JsonIgnore]
	Func<Pt, IRoVar<Pt>, Action<bool>> DragAction
) : IHotspotCmd
{
	public static readonly Func<Pt, IRoVar<Pt>, Action<bool>> EmptyAction = (_, _) => _ => {};
}
