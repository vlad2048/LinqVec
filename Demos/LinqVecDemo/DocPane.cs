using System.Reactive.Linq;
using LinqVec.Logic;
using LinqVec.Utils.WinForms_;
using PowMaybe;
using VectorEditor.Model;
using PowRxVar;
using UILib;
using UILib.Logging;
using VectorEditor;
using WeifenLuo.WinFormsUI.Docking;

namespace LinqVecDemo;

sealed partial class DocPane : DockContent
{
	public IRwMayVar<string> Filename { get; }

	public ModelMan<DocModel> ModelMan { get; private set; } = null!;

	public DocPane((DocModel model, string filename)? load = null)
	{
		InitializeComponent();
		KeyPreview = true;

		Filename = VarMay.Make<string>().D(this);

		if (load?.filename != null)
			Filename.V = May.Some(load.Value.filename);

		this.InitRX(d =>
		{
			ModelMan = vecEditor.InitVectorEditor(load?.model).D(d);

			Filename.Subscribe(filename => Text = filename.Select(Path.GetFileNameWithoutExtension).FailWith("Untitled")).D(d);

			this.Events().KeyDown.Where(e => e.KeyCode == Keys.F4 && e.Control).Subscribe(_ =>
			{
				Close();
			}).D(d);
		});
	}
}