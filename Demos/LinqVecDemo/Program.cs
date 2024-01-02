using LinqVec.Logging;
using LinqVec.Utils;
using LinqVec.Utils.Json;
using LinqVec.Utils.WinForms_;
using LogLib.Structs;
using LogLib.Utils;
using LogLib.Writers;
using PowBasics.Json_;
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
		LR.IdentifyMainThread();



		var gens = VecJsoner.Vec.Load<GenNfo<IWriteSer>[]>(@"C:\tmp\vec\cons\init.json");
		var con = LogVecConKeeper.Instance;
		foreach (var gen in gens)
			con.Gen(gen.Src);
		//ConWriterHistoryUtils.Replay(@"C:\tmp\vec\cons\init.json");




		//IChunk chk = new NewlineChunk();
		/*IChunk[] arr = [
			new TextChunk(new TxtSegment("firsT", Cols.RedBrick)),
			new NewlineChunk(),
			new TextChunk(new TxtSegment("Second", Cols.Doozy)),
		];
		var str = VecJsoner.Vec.Ser(arr);
		File.WriteAllText(@"C:\tmp\vec\cons\tt.json", str);*/

		Application.Run(new MainWin().Track());

		LogAndTellIfThereAreUndisposedDisps();
	}
}

static class Cols
{
	public const int RedBrick = 0x780000;
	public const int Doozy = 0x712345;
}
