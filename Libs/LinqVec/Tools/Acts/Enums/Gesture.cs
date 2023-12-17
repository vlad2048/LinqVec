namespace LinqVec.Tools.Acts.Enums;

[Flags]
public enum Gesture
{
	None = 0,
	Drag = 1,
	Click = 2,
	RightClick = 4,
	DoubleClick = 8,
}

public enum ClickGesture
{
	Click = 2,
	RightClick = 4,
	DoubleClick = 8,
}
