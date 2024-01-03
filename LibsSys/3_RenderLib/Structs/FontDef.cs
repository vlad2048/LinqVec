namespace RenderLib.Structs;

/// <summary>
/// Font definition
/// </summary>
/// <param name="Name">Name</param>
/// <param name="Size">Size in points</param>
/// <param name="Bold">Is bold</param>
/// <param name="Italic">Is italic</param>
public sealed record FontDef(
	string Name,
	float Size,
	bool Bold,
	bool Italic
)
{
	public static readonly FontDef Default = new(
		"Segoe UI",
		9,
		false,
		false
	);
}