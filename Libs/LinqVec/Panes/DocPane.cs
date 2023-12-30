using LinqVec.Logic;
using ReactiveVars;
using System.Reactive.Linq;
using PtrLib;
using UILib;
using WeifenLuo.WinFormsUI.Docking;

namespace LinqVec.Panes
{
	public partial class DocPane<TDoc> : DockContent where TDoc : class
	{
		public IRwVar<Option<string>> Filename { get; }
		public IPtr<TDoc> Doc { get; }

		public DocPane(EditorLogic<TDoc> editorLogic, Option<string> file)
		{
			KeyPreview = true; // otherwise not getting the key events below
			var ctrlD = this.GetD();
			Filename = Var.Make(file, ctrlD);
			Doc = Ptr.Make(editorLogic.LoadOrCreate(file), ctrlD);

			InitializeComponent(Doc, editorLogic.Tools);

			editorLogic.Init(vecEditor.Env, ctrlD);

			this.InitRX(d =>
			{
				Filename.Subscribe(filename => Text = filename.Select(Path.GetFileNameWithoutExtension).IfNone("Untitled")).D(d);
				this.Events().KeyDown.Where(e => e.KeyCode == Keys.F4 && e.Control).Subscribe(_ => Close()).D(d);
				this.Events().KeyDown.Where(e => e.KeyCode == Keys.C).Subscribe(_ => Console.Clear()).D(d);
			});
		}
	}
}
