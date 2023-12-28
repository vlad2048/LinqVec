using PowBasics.CollectionsExt;

namespace LinqVec.Tools.Events;

public record ModKeyState(
	bool Shift,
	bool Alt,
	bool Ctrl
)
{
	public override string ToString()
	{
		if (!Shift && !Alt && !Ctrl) return "_";
		var list = new List<string>();
		if (Shift) list.Add("shift");
		if (Alt) list.Add("alt");
		if (Ctrl) list.Add("ctrl");
		return list.JoinText("+");
	}

	public static readonly ModKeyState Empty = new(
		false,
		false,
		false
	);

	public static ModKeyState Make() => new(
		Control.ModifierKeys.HasFlag(Keys.Shift),
		Control.ModifierKeys.HasFlag(Keys.Alt),
		Control.ModifierKeys.HasFlag(Keys.Control)
	);
}

/*
public interface IModKeys
{
	bool Shift { get; }
	bool Alt { get; }
	bool Ctrl { get; }
}

class ModKeys : IModKeys
{
	public bool Shift => Control.ModifierKeys.HasFlag(Keys.Shift);
	public bool Alt => Control.ModifierKeys.HasFlag(Keys.Alt);
	public bool Ctrl => Control.ModifierKeys.HasFlag(Keys.Control);

	public static readonly IModKeys Instance = new ModKeys();
}
*/