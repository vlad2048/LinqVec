using System.Reactive.Linq;
using LinqVec.Logic;
using PowMaybe;
using VectorEditor.Model;
using PowRxVar;
using UILib;
using VectorEditor;
using WeifenLuo.WinFormsUI.Docking;

namespace LinqVecDemo;

sealed partial class DocPane : DockContent
{
	public IRwMayVar<string> Filename { get; }

	public Model<Doc> Doc { get; private set; } = null!;

	public DocPane((Doc model, string filename)? load = null)
	{
		InitializeComponent();
		KeyPreview = true;

		Filename = VarMay.Make<string>().D(this);

		if (load?.filename != null)
			Filename.V = May.Some(load.Value.filename);

		this.InitRX(d =>
		{
			Doc = vecEditor.InitVectorEditor(load?.model).D(d);

			Filename.Subscribe(filename => Text = filename.Select(Path.GetFileNameWithoutExtension).FailWith("Untitled")).D(d);

			this.Events().KeyDown.Where(e => e.KeyCode == Keys.F4 && e.Control).Subscribe(_ =>
			{
				Close();
			}).D(d);
		});
	}
}