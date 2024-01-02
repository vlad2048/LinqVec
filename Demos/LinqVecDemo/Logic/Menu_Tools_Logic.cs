using LinqVec.Logging;
using LinqVec.Utils;
using LinqVec.Utils.Json;
using PowBasics.Json_;
using ReactiveVars;

namespace LinqVecDemo.Logic;

static class Menu_Tools_Logic
{
	private static SaveFileDialog MkSaveFileDialog() => new()
	{
		DefaultExt = ".json",
		Filter = "Con Files (*.json)|*.json",
		RestoreDirectory = true,
	}; 
	
	public static void Init_Tools_Logic(
		this MainWin win,
		Disp d
	)
	{
		win.menuToolsSaveConsoleOutput.Events().Click.Subscribe(_ =>
		{
			using var dlg = MkSaveFileDialog();
			if (dlg.ShowDialog() != DialogResult.OK) return;
			LogVecConKeeper.Instance.Save(VecJsoner.Vec, dlg.FileName);
		}).D(d);
	}
}