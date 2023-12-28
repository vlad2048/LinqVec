using Geom;
using LinqVec.Tools.Cmds.Enums;

namespace LinqVec.Tools.Cmds;


public static class Cmd
{
	public static ClickHotspotCmd Click(
		string name,
		ClickGesture gesture,
		Func<Option<ToolStateFun>> confirm
	) => new(
		name,
		(Gesture)gesture,
		confirm
	);


	public static DragHotspotCmd Drag(
		string name,
		Action<Pt> action
	) => new(
		name,
		Gesture.Drag,
		action
	);
}