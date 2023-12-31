using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text.Json;
using LinqVec;
using LinqVec.Panes;
using LinqVec.Utils;
using LinqVec.Utils.Rx;
using ReactiveVars;
using UILib.Utils;
using WeifenLuo.WinFormsUI.Docking;

namespace LinqVecDemo.Logic;

static class DocLogic
{
	private static OpenFileDialog MkOpenFileDialog() => new()
	{
		DefaultExt = ".json",
		Filter = "Vec Files (*.json)|*.json",
		RestoreDirectory = true,
	};
	private static SaveFileDialog MkSaveFileDialog() => new()
	{
		DefaultExt = ".json",
		Filter = "Vec Files (*.json)|*.json",
		RestoreDirectory = true,
	}; 
	
	
	public static IRoVar<Option<DocPane<TDoc, TState>>> InitDocLogic(this MainWin win, EditorLogic<TDoc, TState> editorLogic, Disp d)
	{
		var curDoc = win.dockPanel.GetActiveDoc(d);

		curDoc.Enables(win.menuFileSave, win.menuFileSaveAs).D(d);
		win.menuFileNew.Events().Click.Subscribe(_ => New(win.dockPanel, editorLogic)).D(d);
		win.menuFileOpen.Events().Click.Subscribe(_ => Open(win.dockPanel, editorLogic)).D(d);
		win.menuFileSave.Events().Click.Subscribe(_ => Save(false, curDoc.V.Ensure())).D(d);
		win.menuFileSaveAs.Events().Click.Subscribe(_ => Save(true, curDoc.V.Ensure())).D(d);
		win.menuFileExit.Events().Click.Subscribe(_ => win.Close()).D(d);
		win.Events().FormClosing.Subscribe(_ => DisposeAllDocPanes(win)).D(d);

		TrackAndRestoreCurFile(win, curDoc, editorLogic, d);

		return curDoc;
	}


	private static void DisposeAllDocPanes(MainWin win)
	{
		var docPanes = win.dockPanel.Documents.OfType<DocPane<TDoc, TState>>().ToArray();
		foreach (var docPane in docPanes)
			docPane.Dispose();
	}

	private static void TrackAndRestoreCurFile(
		MainWin win,
		IRoVar<Option<DocPane<TDoc, TState>>> curDoc,
		EditorLogic<TDoc, TState> editorLogic,
		Disp d
	)
	{
		var baseName = win.Text;

		OpenLastLoadedFile(win, editorLogic);

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

	private static void OpenLastLoadedFile(MainWin win, EditorLogic<TDoc, TState> editorLogic)
	{
		var hasOpened = false;
		if (win.LastLoadedFile != null && File.Exists(win.LastLoadedFile))
		{
			if (File.Exists(win.LastLoadedFile))
			{
				try
				{
					AddDocPane(win.LastLoadedFile, win.dockPanel, editorLogic);
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
			New(win.dockPanel, editorLogic);
	}


	// **************
	// * New & Open *
	// **************
	private static void New(DockPanel dockPanel, EditorLogic<TDoc, TState> editorLogic) => AddDocPane(null, dockPanel, editorLogic);
	private static void Open(DockPanel dockPanel, EditorLogic<TDoc, TState> editorLogic)
	{
		using var dlg = MkOpenFileDialog();
		if (dlg.ShowDialog() == DialogResult.OK)
			AddDocPane(dlg.FileName, dockPanel, editorLogic);
	}
	private static void AddDocPane(string? filename, DockPanel dockPanel, EditorLogic<TDoc, TState> editorLogic)
	{
		var docPane = filename switch
		{
			null => new DocPane<TDoc, TState>(editorLogic, None),
			not null => new DocPane<TDoc, TState>(editorLogic, Some(filename))
		};
		docPane.Show(dockPanel, DockState.Document);
		Rx.Sched.Schedule(() => docPane.vecEditor.Focus());
	}


	// ********
	// * Save *
	// ********
	private static void Save(bool saveAs, DocPane<TDoc, TState> docPane)
	{
		var filename = (saveAs || docPane.Filename.V.IsNone) switch {
			true => AskSaveFilename(),
			false => docPane.Filename.V
		};
		filename.IfSome(docPane.Save);
	}
	private static Option<string> AskSaveFilename()
	{
		using var dlg = MkSaveFileDialog();
		return (dlg.ShowDialog() == DialogResult.OK) switch {
			true => dlg.FileName,
			false => None
		};
	}


	// ********
	// * Misc *
	// ********
	private static IRoVar<Option<DocPane<TDoc, TState>>> GetActiveDoc(this DockPanel dockPanel, Disp d)
	{
		var activeDoc = Var.Make(Option<DocPane<TDoc, TState>>.None, d);
		dockPanel.WhenActiveDocChanged().Subscribe(_ =>
		{
			activeDoc.V = dockPanel.ActiveDocument as DocPane<TDoc, TState>;
		}).D(d);
		return activeDoc;
	}
	private static IObservable<Unit> WhenActiveDocChanged(this DockPanel dockPanel) => Obs.FromEventPattern(e => dockPanel.ActiveDocumentChanged += e, e => dockPanel.ActiveDocumentChanged -= e).ToUnit();
}