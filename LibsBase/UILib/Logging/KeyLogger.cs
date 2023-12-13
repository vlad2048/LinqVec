/*
namespace UILib.Logging;

public static class KeyLogger
{
	public static IDisposable LogKeys(this Control ctrl)
	{
		var name = ctrl.GetType().Name;
		var d = new Disp();
		ctrl.Events().KeyDown.Subscribe(e => L($"[{name}].KeyDown({e.KeyCode}  ctrl:{e.Control})")).D(d);
		ctrl.Events().PreviewKeyDown.Subscribe(e => L($"[{name}].PreviewKeyDown({e.KeyCode}  ctrl:{e.Control})")).D(d);
		return d;
	}

	private static void L(string s) => Console.WriteLine(s);
}
*/