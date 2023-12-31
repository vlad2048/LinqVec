using ReactiveVars;
using System.Reactive.Linq;
using PtrLib;
using UILib;
using WeifenLuo.WinFormsUI.Docking;

namespace LinqVec.Panes;

public partial class DocPane<TDoc, TState> : DockContent where TDoc : class
{
	private readonly EditorLogic<TDoc, TState> editorLogic;

	public IRwVar<Option<string>> Filename { get; }
	public IPtr<TDoc> Doc { get; }

	public DocPane(EditorLogic<TDoc, TState> editorLogic, Option<string> file)
	{
		this.editorLogic = editorLogic;
		KeyPreview = true; // otherwise we're not getting the key events below
		var ctrlD = this.GetD();
		Filename = Var.Make(file, ctrlD);
		var docInit = editorLogic.LoadOrCreate(file);

		InitializeComponent(docInit, editorLogic);

		var env = vecEditor.Env;
		Doc = env.Doc;
		editorLogic.Init(env, ctrlD);

		this.InitRX(d =>
		{
			Filename.Subscribe(filename => Text = filename.Select(Path.GetFileNameWithoutExtension).IfNone("Untitled")).D(d);
			this.Events().KeyDown.Where(e => e.KeyCode == Keys.F4 && e.Control).Subscribe(_ => Close()).D(d);
			this.Events().KeyDown.Where(e => e.KeyCode == Keys.C).Subscribe(_ => Console.Clear()).D(d);
		});
	}

	public void Save(string filename) => editorLogic.Save(filename, Doc.V);
}