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

	public static ClickHotspotCmd Click(
		string name,
		ClickGesture gesture,
		Action confirm
	) => new(
		name,
		(Gesture)gesture,
		() =>
		{
			confirm();
			return None;
		}
	);


	public static DragHotspotCmd Drag(
		string name,
		Func<Pt, Action> action
	) => new(
		name,
		Gesture.Drag,
		action
	);
}