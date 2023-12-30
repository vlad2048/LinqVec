using LinqVec.Utils.WinForms_;
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
		LT.IdentifyMainThread();

		Application.Run(new MainWin().Track());

		LogAndTellIfThereAreUndisposedDisps();
	}
}
