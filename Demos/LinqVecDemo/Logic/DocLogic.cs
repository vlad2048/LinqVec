using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using LinqVec.Utils.Json;
using LinqVec.Utils.Rx;
using PowBasics.Json_;
using ReactiveVars;
using UILib.Utils;
using VectorEditor.Model;
using WeifenLuo.WinFormsUI.Docking;

namespace LinqVecDemo.Logic;

static class DocLogic
{
	public static IRoVar<Option<DocPane>> InitDocLogic(this MainWin win, Disp d)
	{
		var activeDoc = win.dockPanel.GetActiveDoc(d);
		var baseName = win.Text;


		win.menuFileNew.Events().Click.Subscribe(_ => Open(false)).D(d);
		win.menuFileOpen.Events().Click.Subscribe(_ => Open(true)).D(d);
		win.menuFileSave.Events().Click.Subscribe(_ => Save(false)).D(d);
		win.menuFileSaveAs.Events().Click.Subscribe(_ => Save(true)).D(d);
		win.menuFileExit.Events().Click.Subscribe(_ => win.Close()).D(d);

		var hasOpened = false;
		if (win.LastLoadedFile != null)
		{
			if (File.Exists(win.LastLoadedFile))
			{
				var doc = OpenFile(win.LastLoadedFile);
				doc.Show(win.dockPanel, DockState.Document);
				doc.Focus();
				hasOpened = true;
			}
			else
			{
				win.LastLoadedFile = null;
			}
		}
		if (!hasOpened)
			Open(false);

		activeDoc
			.Where(e => e.IsNone)
			.Subscribe(_ => win.LastLoadedFile = null).D(d);
		

		activeDoc.Enables(win.menuFileSave, win.menuFileSaveAs).D(d);


		void SetTitle(Option<string> mayFilename) => win.Text = mayFilename.Match(
			f => $"{baseName} - [{Path.GetFileNameWithoutExtension(f)}]",
			baseName
		);
		activeDoc.Select(e =>
			from doc in e
			from file in doc.Filename.V
			select file
		).Subscribe(SetTitle).D(d);

		
		DocPane OpenFile(string file)
		{
			var m = VecJsoner.Default.Load<Doc>(file);
			var doc = new DocPane((m, file));
			win.LastLoadedFile = file;
			return doc;
		}

		void Open(bool fromFile)
		{
			DocPane doc;
			if (fromFile)
			{
				using var dlg = new OpenFileDialog
				{
					DefaultExt = ".json",
					Filter = "Vec Files (*.json)|*.json",
					RestoreDirectory = true,
				};
				if (dlg.ShowDialog() == DialogResult.OK)
				{
					doc = OpenFile(dlg.FileName);
				}
				else
				{
					return;
				}
			}
			else
			{
				doc = new DocPane();
			}
			doc.Show(win.dockPanel, DockState.Document);
			Rx.Sched.Schedule(() => doc.vecEditor.Focus());
		}


		void Save(bool saveAs)
		{
			var doc = activeDoc.V.IfNone(() => throw new ArgumentException());
			var m = doc.Doc.V;

			if (saveAs || doc.Filename.V.IsNone)
			{
				using var dlg = new SaveFileDialog
				{
					DefaultExt = ".json",
					Filter = "Vec Files (*.json)|*.json",
					RestoreDirectory = true,
				};
				if (dlg.ShowDialog() == DialogResult.OK)
				{
					VecJsoner.Default.Save(dlg.FileName, m);
					doc.Filename.V = dlg.FileName;
				}
			}
			else
			{
				var filename = doc.Filename.V.IfNone(() => throw new ArgumentException());
				VecJsoner.Default.Save(filename, m);
			}
		}
		


		return activeDoc;
	}

	private static IRoVar<Option<DocPane>> GetActiveDoc(this DockPanel dockPanel, Disp d)
	{
		var activeDoc = Var.Make(Option<DocPane>.None, d);
		dockPanel.WhenActiveDocChanged().Subscribe(v =>
		{
			activeDoc.V = dockPanel.ActiveDocument as DocPane;
		}).D(d);
		return activeDoc;
	}

	private static IObservable<Unit> WhenActiveDocChanged(this DockPanel dockPanel) => Obs.FromEventPattern(e => dockPanel.ActiveDocumentChanged += e, e => dockPanel.ActiveDocumentChanged -= e).ToUnitExt();
}