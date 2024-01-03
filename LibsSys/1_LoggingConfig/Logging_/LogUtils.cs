using PowBasics.ColorCode.Utils;

namespace LoggingConfig.Logging_;

public static class LogUtils
{
	public static void L(string s)
	{
		ConUtils.Write(Thread, C.Thread);
		ConUtils.Write($"{Time} ", C.Time);
		ConUtils.WriteLine(s, C.Text);
	}

	public static void LIf(bool condition, string s)
	{
		if (condition)
			L(s);
	}

	public static void LTitle(string s)
	{
		L("");
		L(s);
		L(new string('=', s.Length));
	}

	private static string Thread {
		get {
			var thread = System.Threading.Thread.CurrentThread;
			var name = thread.Name ?? "(unnamed)";
			if (name == ".NET ThreadPool Worker") name = "ThreadPool";
			name = name.PadRight(10);

			var str = $"{thread.ManagedThreadId}/{name}";
			return $"[{str}]";
		}
	}

	private static string Time => $"[{DateTime.Now:HH:mm:ss.fff}]";

	// ReSharper disable MemberHidesStaticFromOuterClass
	private static class C
	{
		public static readonly Color Thread = Color.FromArgb(128, 0, 128);
		public static readonly Color Time = Color.FromArgb(141, 141, 28);
		public static readonly Color Text = Color.FromArgb(30, 144, 255);
	}
	// ReSharper restore MemberHidesStaticFromOuterClass
}