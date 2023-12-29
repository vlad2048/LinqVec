using LinqVec.Logic;
using ReactiveVars;
using System.Reactive.Linq;
using UILib;
using WeifenLuo.WinFormsUI.Docking;

namespace LinqVec.Panes
{
	public partial class DocPane<TDoc> : DockContent where TDoc : class
	{
		public IRwVar<Option<string>> Filename { get; }

		public Unmod<TDoc> Doc { get; private set; } = null!;

		public DocPane(EditorLogic<TDoc> editorLogic, Option<string> file)
		{
			InitializeComponent();
			KeyPreview = true;
			var ctrlD = this.GetD();
			Filename = Var.Make(file, ctrlD);

			this.InitRX(d =>
			{
				Doc = new Unmod<TDoc>(editorLogic.LoadOrCreate(file), d);
				editorLogic.Init(vecEditor, Doc, d);
				vecEditor.Init(new VecEditorInitNfo<TDoc>(Doc, editorLogic.Tools));

				Filename.Subscribe(filename => Text = filename.Select(Path.GetFileNameWithoutExtension).IfNone("Untitled")).D(d);
				this.Events().KeyDown.Where(e => e.KeyCode == Keys.F4 && e.Control).Subscribe(_ => Close()).D(d);
				this.Events().KeyDown.Where(e => e.KeyCode == Keys.C).Subscribe(_ => Console.Clear()).D(d);
			});
		}
	}
}
