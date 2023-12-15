using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using PowRxVar;
using UILib;

namespace LinqVec.Utils;

public static class Logger
{
	public static void Write(string s) => Console.Write(s);
	public static void WriteLine(string s) => Console.WriteLine(s);
	public static void WriteLine() => Console.WriteLine();

	public static IObservable<T> Log<T>(this IObservable<T> obs, IRoDispBase d, [CallerArgumentExpression(nameof(obs))] string? obsStr = null)
	{
		Disposable.Create(() => WriteLine($"{obsStr} <- Dispose()")).D(d);
		obs.Subscribe(v => WriteLine($"{obsStr} <- {v}")).D(d);
		return obs;
	}

	public static void Log(this IRoDispBase d, string name)
	{
		WriteLine($"[{name}].new()");
		Disposable.Create(() => WriteLine($"[{name}].Dispose()")).D(d);
	}

	public static IDisposable AddLabel<T>(this StatusStrip strip, string label, IObservable<T> obs)
	{
		var d = new Disp();
		var labelHeader = new ToolStripLabel($"{label}:")
		{
			Font = C.Fonts.MonoHeader,
		};
		var labelValue = new ToolStripStatusLabel("")
		{
			Font = C.Fonts.MonoValue,
			BorderSides = ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom,
			BorderStyle = Border3DStyle.Raised,
			Margin = new Padding(-5, 0, 5, 0),
		};
		obs
			//.ObserveOnUI()
			.Subscribe(v => labelValue.Text = $"{v}").D(d);
		strip.Items.AddRange(new ToolStripItem[]
		{
			labelHeader,
			labelValue
		});
		return d;
	}
}