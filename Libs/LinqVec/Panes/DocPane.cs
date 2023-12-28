using LinqVec.Logic;
using LinqVec.Panes.DocPaneLogic_;
using ReactiveVars;
using System.Reactive.Linq;
using LinqVec.Utils;
using UILib;
using WeifenLuo.WinFormsUI.Docking;

namespace LinqVec.Panes
{
	public partial class DocPane<TDoc> : DockContent where TDoc : class
	{
		public IRwVar<Option<string>> Filename { get; }

		public IUndoerReadOnly<TDoc> Doc { get; private set; } = null!;

		public DocPane(EditorLogic<TDoc> editorLogic, Option<DocPaneLoadInfo<TDoc>> loadInfo)
		{
			InitializeComponent();

			KeyPreview = true;

			var ctrlD = this.GetD();
			Filename = Var.Make(Option<string>.None, ctrlD);


			Filename.V = loadInfo.Map(e => e.Filename);

			this.InitRX(d =>
			{
				Doc = editorLogic.Init(vecEditor, loadInfo.Map(e => e.Doc).ToNullable(), d);

				Filename
					.Subscribe(filename => Text = filename.Select(Path.GetFileNameWithoutExtension).IfNone("Untitled")).D(d);

				this.Events().KeyDown.Where(e => e.KeyCode == Keys.F4 && e.Control).Subscribe(_ => Close()).D(d);

				this.Events().KeyDown.Where(e => e.KeyCode == Keys.C).Subscribe(_ => Console.Clear()).D(d);
			});
		}
	}
}
