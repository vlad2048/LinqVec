using ReactiveVars;
using System.Reactive.Linq;
using LinqVec.Structs;
using LinqVec.Tools;
using PtrLib;
using UILib;
using WeifenLuo.WinFormsUI.Docking;
using Microsoft.VisualBasic.Logging;

namespace LinqVec.Panes;

public partial class DocPane : DockContent
{
	public IRwVar<Option<string>> Filename { get; }
	public ToolEnv Env => vecEditor.Env;
	public EditorLogic Logic => vecEditor.Logic;
	public IDocHolder Doc => Logic.DocHolder;

	public DocPane(EditorLogicMaker maker, Option<string> file)
	{
		KeyPreview = true; // otherwise we're not getting the key events below
		var ctrlD = this.GetD();
		Filename = Var.Make(file, ctrlD);

		InitializeComponent(maker, file);

		this.InitRX(d =>
		{
			Filename.Subscribe(filename => Text = filename.Select(Path.GetFileNameWithoutExtension).IfNone("Untitled")).D(d);
			this.Events().KeyDown.Where(e => e.KeyCode == Keys.F4 && e.Control).Subscribe(_ => Close()).D(d);
			this.Events().KeyDown.Where(e => e.KeyCode == Keys.C).Subscribe(_ => Console.Clear()).D(d);
		});
	}

	public void Save(string filename)
	{
		Logic.Save(filename);
		Filename.V = filename;
	}
}