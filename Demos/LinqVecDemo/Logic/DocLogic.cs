﻿using System.Reactive;
using System.Reactive.Linq;
using LinqVec.Utils.Json;
using PowBasics.Json_;
using PowMaybe;
using PowRxVar;
using UILib.Utils;
using VectorEditor.Model;
using WeifenLuo.WinFormsUI.Docking;

namespace LinqVecDemo.Logic;

static class DocLogic
{
	public static (IRoMayVar<DocPane>, IDisposable) InitDocLogic(this MainWin win)
	{
		var d = new Disp();
		var activeDoc = win.dockPanel.GetActiveDoc().D(d);
		var baseName = win.Text;


		win.menuFileNew.Events().Click.Subscribe(_ => Open(false)).D(d);
		win.menuFileOpen.Events().Click.Subscribe(_ => Open(true)).D(d);
		win.menuFileSave.Events().Click.Subscribe(_ => Save(false)).D(d);
		win.menuFileSaveAs.Events().Click.Subscribe(_ => Save(true)).D(d);
		win.menuFileExit.Events().Click.Subscribe(_ => win.Close()).D(d);

		if (win.LastLoadedFile != null)
		{
			if (File.Exists(win.LastLoadedFile))
			{
				var doc = OpenFile(win.LastLoadedFile);
				doc.Show(win.dockPanel, DockState.Document);
			}
			else
			{
				win.LastLoadedFile = null;
			}
		}
		activeDoc.WhenNone().Subscribe(_ => win.LastLoadedFile = null).D(d);


		activeDoc.Enables(win.menuFileSave, win.menuFileSaveAs).D(d);


		void SetTitle(Maybe<string> mayFilename) => win.Text = mayFilename.IsSome(out var filename) switch
		{
			true => $"{baseName} - [{Path.GetFileNameWithoutExtension(filename)}]",
			false => baseName
		};
		activeDoc.Select(e =>
			from doc in e
			from file in doc.Filename.V
			select file
		).Subscribe(SetTitle).D(d);


		DocPane OpenFile(string file)
		{
			var m = VecJsoner.Default.Load<DocModel>(file);
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
		}


		void Save(bool saveAs)
		{
			var doc = activeDoc.V.Ensure();
			var m = doc.ModelMan.V;

			if (saveAs || doc.Filename.V.IsNone(out var filename))
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
					doc.Filename.V = May.Some(dlg.FileName);
				}
			}
			else
			{
				VecJsoner.Default.Save(filename, m);
			}
		}
		


		return (activeDoc, d);
	}

	private static (IRoMayVar<DocPane>, IDisposable) GetActiveDoc(this DockPanel dockPanel)
	{
		var d = new Disp();
		var activeDoc = VarMay.Make<DocPane>(
			dockPanel.WhenActiveDocChanged()
				.Select(_ => (dockPanel.ActiveDocument as DocPane).ToMaybe())
		).D(d);
		return (activeDoc, d);
	}

	private static IObservable<Unit> WhenActiveDocChanged(this DockPanel dockPanel) => Obs.FromEventPattern(e => dockPanel.ActiveDocumentChanged += e, e => dockPanel.ActiveDocumentChanged -= e).ToUnit();
}