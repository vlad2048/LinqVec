using System.Drawing;
using System.Reflection;
using LogLib.Structs;

namespace LogLib;


public static class S
{
	public static class General
	{
		public static readonly NamedColor Black = new(Color.FromArgb(0x000000));
		public static readonly NamedColor White = new(Color.FromArgb(0xffffff));
		public static readonly NamedColor Neutral = new(Color.FromArgb(0x7c7c7c));
		public static readonly NamedColor On = new(Color.FromArgb(0x5fba32));
		public static readonly NamedColor Off = new(Color.FromArgb(0x418520));
	}

	public static class LogTicker
	{
		public static readonly NamedColor Cnt = new(Color.FromArgb(0x404040));
		public static readonly NamedColor BackAlert = new(Color.FromArgb(0xe879d5));
		public static readonly NamedColor TitleBarFore = new(Color.FromArgb(0xb0b0b0));
		public static readonly NamedColor TitleBarBack = new(Color.FromArgb(0x303030));

		public static class Time
		{
			public static readonly NamedColor Val0 = new(Color.FromArgb(0x4ff03a));
			public static readonly NamedColor Val1 = new(Color.FromArgb(0xafe356));
			public static readonly NamedColor Val2 = new(Color.FromArgb(0xd2e055));
			public static readonly NamedColor Val3 = new(Color.FromArgb(0xdec754));
			public static readonly NamedColor Val4 = new(Color.FromArgb(0xdea74e));
			public static readonly NamedColor Val5 = new(Color.FromArgb(0xd17b41));
			public static readonly NamedColor Val6 = new(Color.FromArgb(0xd6583c));
			public static readonly NamedColor Val7 = new(Color.FromArgb(0xde4343));
		}
	}

	public static class Evt
	{
		public static readonly NamedColor Main = new(Color.FromArgb(0x65218a));
	}
	public static class Usr
	{
		public static readonly NamedColor Main = new(Color.FromArgb(0xa635c3));
		public static readonly NamedColor Fast = new(Color.FromArgb(0x4e9b29));
	}
	public static class Cmd
	{
		public static readonly NamedColor Main = new(Color.FromArgb(0xf55ff0));
		public static readonly NamedColor DragStart = new(Color.FromArgb(0xe6e835));
		public static readonly NamedColor DragFinish = new(Color.FromArgb(0x3be341));
		public static readonly NamedColor Confirm = new(Color.FromArgb(0x3be341));
		public static readonly NamedColor Shortcut = new(Color.FromArgb(0x2786cf));
		public static readonly NamedColor Cancel = new(Color.FromArgb(0xf55067));
	}
	public static class Mod
	{
		public static readonly NamedColor Start = new(Color.FromArgb(0xbbe04c));
		public static readonly NamedColor Commit = new(Color.FromArgb(0x4ce659));
		public static readonly NamedColor Cancel = new(Color.FromArgb(0xe8554a));
		public static readonly NamedColor Name = new(Color.FromArgb(0x4dbbeb));
		public static readonly NamedColor Model = new(Color.FromArgb(0x4b6069));
	}

	public static class Mouse
	{
		public static readonly NamedColor Down = new(Color.FromArgb(0x28affc));
		public static readonly NamedColor Up = new(Color.FromArgb(0x757575));
		public static readonly NamedColor OutsideFore = new(Color.FromArgb(0x41efe8));
		public static readonly NamedColor OutsideBack = new(Color.FromArgb(0xeb5f73));
	}

	public static class Misc
	{
		public static readonly NamedColor Keys = new(Color.FromArgb(0x0f6e0f));
		public static readonly NamedColor MousePos = new(Color.FromArgb(0xcacaca));


		public static readonly NamedColor Time = new(Color.FromArgb(0xbf9822));
	}




	private static bool isInit;
	public static void Init()
	{
		if (isInit) return;
		isInit = true;
		void Rec(Type node, string name)
		{
			var subNodes = node.GetNestedTypes(BindingFlags.Static | BindingFlags.Public);
			foreach (var subNode in subNodes)
				Rec(subNode, $"{name}.{subNode.Name}");
			var fields = node.GetFields(BindingFlags.Static | BindingFlags.Public).Where(e => e.FieldType == typeof(NamedColor)).ToArray();
			foreach (var field in fields)
			{
				var namedColor = (NamedColor)field.GetValue(null)!;
				var colorName = $"{name}.{field.Name}";
				namedColor.Name = colorName;
			}
		}
		var root = typeof(S);
		Rec(root, root.Name);
	}
}
