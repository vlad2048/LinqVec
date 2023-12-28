using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text.Json;
using LinqVec.Utils;
using LinqVec.Utils.Json;
using LinqVec.Utils.Rx;
using PowBasics.Json_;
using ReactiveVars;
using UILib.Utils;
using WeifenLuo.WinFormsUI.Docking;
using Doc = VectorEditor.Model.Doc;

namespace LinqVecDemo.Logic;

static class DocLogic
{
	public static IRoVar<Option<DocPane>> InitDocLogic(this MainWin win, Disp d)
	{
		var curDoc = win.dockPanel.GetActiveDoc(d);

		curDoc.Enables(win.menuFileSave, win.menuFileSaveAs).D(d);
		win.menuFileNew.Events().Click.Subscribe(_ => New(win.dockPanel)).D(d);
		win.menuFileOpen.Events().Click.Subscribe(_ => Open(win.dockPanel)).D(d);
		win.menuFileSave.Events().Click.Subscribe(_ => Save(false, curDoc)).D(d);
		win.menuFileSaveAs.Events().Click.Subscribe(_ => Save(true, curDoc)).D(d);
		win.menuFileExit.Events().Click.Subscribe(_ => win.Close()).D(d);
		win.Events().Closing.Subscribe(_ => DisposeAllDocPanes(win)).D(d);

		TrackAndRestoreCurFile(win, curDoc, d);

		return curDoc;
	}


	private static void DisposeAllDocPanes(MainWin win)
	{
		var docPanes = win.dockPanel.Documents.OfType<DocPane>().ToArray();
		foreach (var docPane in docPanes)
			docPane.Dispose();
	}


	private static void TrackAndRestoreCurFile(
		MainWin win,
		IRoVar<Option<DocPane>> curDoc,
		Disp d
	)
	{
		var baseName = win.Text;

		OpenLastLoadedFile(win);

		var curFile =
			curDoc
				.Select(e => e.Match(
					f => f.Filename,
					() => Obs.Return(Option<string>.None)
				))
				.Switch()
				.Select(e => e.ToNullable());

		curFile
			.Subscribe(e =>
			{
				win.LastLoadedFile = e;
				win.Text = e switch
				{
					null => baseName,
					not null => $"{baseName} - [{Path.GetFileNameWithoutExtension(e)}]"
				};
			}).D(d);

		win.statusStrip.AddLabel("File", curFile.Select(e => e ?? "_")).D(d);
	}



	private static void OpenLastLoadedFile(MainWin win)
	{
		var hasOpened = false;
		if (win.LastLoadedFile != null && File.Exists(win.LastLoadedFile))
		{
			if (File.Exists(win.LastLoadedFile))
			{
				try
				{
					AddDocPane(win.LastLoadedFile, win.dockPanel);
					hasOpened = true;
				}
				catch (JsonException)
				{
				}
			}
			else
			{
				win.LastLoadedFile = null;
			}
		}
		if (!hasOpened)
			New(win.dockPanel);
	}



	private static void New(DockPanel dockPanel) => AddDocPane(null, dockPanel);

	private static void Open(DockPanel dockPanel)
	{
		using var dlg = new OpenFileDialog
		{
			DefaultExt = ".json",
			Filter = "Vec Files (*.json)|*.json",
			RestoreDirectory = true,
		};
		if (dlg.ShowDialog() == DialogResult.OK)
			AddDocPane(dlg.FileName, dockPanel);
	}

	private static void Save(bool saveAs, IRoVar<Option<DocPane>> curDoc)
	{
		var doc = curDoc.V.IfNone(() => throw new ArgumentException());
		var m = doc.Doc.CurReadOnly;

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
				VecJsoner.Vec.Save(dlg.FileName, m.V);
				doc.Filename.V = dlg.FileName;
			}
		}
		else
		{
			var filename = doc.Filename.V.IfNone(() => throw new ArgumentException());
			VecJsoner.Vec.Save(filename, m.V);
		}
	}


	private static void AddDocPane(string? filename, DockPanel dockPanel)
	{
		var docPane = filename switch {
			null => new DocPane(),
			not null => new DocPane((VecJsoner.Vec.Load<Doc>(filename), filename))
		};
		docPane.Show(dockPanel, DockState.Document);
		Rx.Sched.Schedule(() => docPane.vecEditor.Focus());
	}



	private static IRoVar<Option<DocPane>> GetActiveDoc(this DockPanel dockPanel, Disp d)
	{
		var activeDoc = Var.Make(Option<DocPane>.None, d);
		dockPanel.WhenActiveDocChanged().Subscribe(_ =>
		{
			activeDoc.V = dockPanel.ActiveDocument as DocPane;
		}).D(d);
		return activeDoc;
	}

	private static IObservable<Unit> WhenActiveDocChanged(this DockPanel dockPanel) => Obs.FromEventPattern(e => dockPanel.ActiveDocumentChanged += e, e => dockPanel.ActiveDocumentChanged -= e).ToUnit();
}