using LogLib;
using LogLib.Interfaces;

namespace LinqVec.Logging.Utils;

public static class WriteUtilsExt
{
	public static string GetBaseName(this object obj, string suffix) => obj.GetType().Name.RemoveSuffix(suffix);

	public static ITxtWriter Key(this ITxtWriter w, Keys key) => w
		.Write("key:", LinqVecColors.General_Gray)
		.Write($"{key}", LinqVecColors.General_Gray);

	public static ITxtWriter Cmd(this ITxtWriter w, string cmd) => w
		.Write("cmd:", LinqVecColors.General_Gray)
		.Write($"{cmd}", LinqVecColors.Hotspot_Cmd);

	public static ITxtWriter Shortcut(this ITxtWriter w, string shortcut) => w
		.Write("shortcut:", LinqVecColors.General_Gray)
		.Write($"{shortcut}", LinqVecColors.Hotspot_Shortcut);


	private static string RemoveSuffix(this string str, string suffix) => str.EndsWith(suffix) switch
	{
		true => str[..^suffix.Length],
		false => str
	};
}