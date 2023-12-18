using System.Reactive.Linq;
using LinqVec.Logic;
using ReactiveVars;
using VectorEditor.Model;
using UILib;
using VectorEditor;
using WeifenLuo.WinFormsUI.Docking;

namespace LinqVecDemo;

sealed partial class DocPane : DockContent
{
	public IRwVar<Option<string>> Filename { get; }

	public Model<Doc> Doc { get; private set; } = null!;

	public DocPane((Doc model, string filename)? load = null)
	{
		InitializeComponent();
		KeyPreview = true;

		var ctrlD = this.GetD();
		Filename = Var.Make(Option<string>.None, ctrlD);

		if (load?.filename != null)
			Filename.V = load.Value.filename;

		this.InitRX(d =>
		{
			Doc = vecEditor.InitVectorEditor(load?.model, d);

			Filename
				.Subscribe(filename => Text = filename.Select(Path.GetFileNameWithoutExtension).IfNone("Untitled")).D(d);

			this.Events().KeyDown.Where(e => e.KeyCode == Keys.F4 && e.Control).Subscribe(_ => Close()).D(d);

			this.Events().KeyDown.Where(e => e.KeyCode == Keys.C).Subscribe(_ => Console.Clear()).D(d);

		});
	}
}