namespace LinqVec.Tools.Cmds.Enums;

[Flags]
public enum Gesture
{
	None = 0,
	Drag = 1,
	Click = 2,
	RightClick = 4,
	DoubleClick = 8,
	ShiftClick = 16,
}

public enum ClickGesture
{
	Click = Gesture.Click,
	RightClick = Gesture.RightClick,
	DoubleClick = Gesture.DoubleClick,
	ShiftClick = Gesture.ShiftClick,
}
