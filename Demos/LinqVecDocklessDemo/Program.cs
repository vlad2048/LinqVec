using UILib;

namespace LinqVecDocklessDemo;

static class Program
{
	[STAThread]
	static void Main()
	{
		ApplicationConfiguration.Initialize();
		//ConUtils.Init();
		LR.IdentifyMainThread();

		Application.Run(new MainWin().Track());

		LogAndTellIfThereAreUndisposedDisps();
	}
}