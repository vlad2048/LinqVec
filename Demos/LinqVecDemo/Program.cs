using System.Reactive.Linq;
using LinqVec.Utils.WinForms_;
using PowRxVar;
using UILib;

namespace LinqVecDemo;

static class Program
{
	/// <summary>
	///  The main entry point for the application.
	/// </summary>
	[STAThread]
	static void Main()
	{
		// To customize application configuration such as set high DPI settings or default font,
		// see https://aka.ms/applicationconfiguration.
		ApplicationConfiguration.Initialize();
		ConUtils.Init();


		Application.Run(new MainWin().Track());

		CheckForUndisposedDisps();

		/*var testD = new Disp();
		AppDomain.CurrentDomain.ProcessExit += (_, _) =>
		{
			testD.Dispose();
		};

		var obs = Obs.Interval(TimeSpan.FromMilliseconds(300));
		var obsConn = obs.Replay(1);
		obsConn.Connect().D(testD);
		obsConn.Subscribe(e => Console.WriteLine($"e <- {e}"));*/
	}
}
