using System.Reactive.Linq;
using CoolColorPicker.Logic;
using CoolColorPicker.Structs;
using CoolColorPicker.Utils;
using ReactiveVars;
using UILib;

namespace CoolColorPicker;




public partial class ColorPickerDialog : Form
{
	public IRwVar<Color> Color { get; }

	public ColorPickerDialog()
	{
		InitializeComponent();
		SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);

		var ctrlD = this.GetD();
		var colorRw = Var.MakeBound(Consts.DefaultColor, ctrlD);


		Color = colorRw;


		this.InitRX(d =>
		{
			this.Events().Load.Subscribe(_ => this.Track()).D(d);
			this.Events().Activated.Subscribe(_ => okButton.Focus()).D(d);

			Color.Subscribe(e => previewPanel.BackColor = e).D(d);

			var hsvaCtrl = new HSVATrackBars(
				new Duo(hueTrackBar, hueUpDown),
				new Duo(satTrackBar, satUpDown),
				new Duo(valTrackBar, valUpDown),
				new Duo(alphaTrackBar, alphaUpDown)
			);
			var rgbaCtrl = rgbPickPanel;

			colorRw.WhenOuter.Prepend(colorRw.V).Subscribe(e =>
			{
				var evt = new ColorUpdateEvt(Src.ColorOutput, e.ToRgba());
				hsvaCtrl.Set(evt);
				rgbaCtrl.Set(evt);
			}).D(d);

			hsvaCtrl.WhenUpdate.Subscribe(e =>
			{
				colorRw.SetInner(e.Color.ToColor());
				rgbaCtrl.Set(e);
			}).D(d);

			rgbaCtrl.WhenUpdate.Subscribe(e =>
			{
				colorRw.SetInner(e.Color.ToColor());
				hsvaCtrl.Set(e);
			}).D(d);




			// ***************
			// * OK & Cancel *
			// ***************
			okButton.Events().Click.Subscribe(_ =>
			{
				DialogResult = DialogResult.OK;
				Close();
			}).D(d);

			cancelButton.Events().Click
				.Subscribe(_ =>
				{
					DialogResult = DialogResult.Cancel;
					Close();
				}).D(d);
		});
	}
}




















/*
public partial class ColorPickerDialog : Form
{
	public IRwVar<Color> Color { get; }

	public ColorPickerDialog()
	{
		InitializeComponent();
		SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);

		var ctrlD = this.GetD();
		var colorRw = Var.MakeBound(Consts.DefaultColor, ctrlD);
		Color = colorRw;


		this.InitRX(d =>
		{
			this.Events().Load.Subscribe(_ => this.Track()).D(d);
			this.Events().Activated.Subscribe(_ => okButton.Focus()).D(d);

			// *********
			// * Hooks *
			// *********
			HookRgbPos(out var rgbPos, rgbPickPanel, d);
			HookHsv(out var hsv, hueTrackBar, hueUpDown, satTrackBar, satUpDown, valTrackBar, valUpDown, d);
			HookValue(out var alpha, alphaTrackBar, alphaUpDown, d);


			// ********************
			// * Update crosshair *
			// ********************
			rgbPos.Subscribe(e => rgbPickPanel.SetCrosshairPos(e.Pos)).D(d);
			var hsv2Crosshair = true;
			hsv.Where(_ => hsv2Crosshair).Subscribe(e => rgbPickPanel.SetCrosshairPosFromRgb(ColorUtils.Hsv2Rgb(e))).D(d);


			// ***************************
			// * Compute the final Color *
			// ***************************
			// HACK: StackOverflow
			//Observable.Merge(
			//		rgbPos.Select(e => Col.FromArgb(alpha.V, e.Rgb.R, e.Rgb.G, e.Rgb.B)),
			//		hsv.Where(_ => hsv2Crosshair).Select(e => ColorUtils.HsvA2Col(e, alpha.V)),
			//		alpha.Select(e => Col.FromArgb(e, Color.V.R, Color.V.G, Color.V.B)),
			//		WhenInitialColor
			//			.Do(e => cancelColor = e),
			//		Observable.Merge(
			//				cancelButton.Events().Click.ToUnit(),
			//				this.Events().FormClosing.Where(_ => DialogResult != DialogResult.OK).ToUnit()
			//			)
			//			.Select(_ => cancelColor)
			//	)
			//	.Subscribe(e => colorRw.V = e).D(d);


			// *************************************************************************
			// * Keep previewPanel & hsv & alpha & crosshair from InitialColor in sync *
			// *************************************************************************
			Color.Subscribe(e => previewPanel.BackColor = e).D(d);

			Observable.Merge(
					rgbPos.Select(rgb => ColorUtils.Col2HsvA(Col.FromArgb(255, rgb.Rgb.R, rgb.Rgb.G, rgb.Rgb.B)).Item1),
					WhenInitialColor.Select(col => ColorUtils.Col2HsvA(col).Item1)
				)
				.Subscribe(e =>
				{
					hsv2Crosshair = false;
					hsv.V = e;
					hsv2Crosshair = true;
				}).D(d);

			Color.Subscribe(e =>
			{
				alpha.V = e.A;
				Refresh();
			}).D(d);

			WhenInitialColor.Subscribe(e => rgbPickPanel.SetCrosshairPosFromRgb(new Rgb(e.R, e.G, e.B))).D(d);

			// ***************
			// * OK & Cancel *
			// ***************
			okButton.Events().Click.Subscribe(_ =>
			{
				DialogResult = DialogResult.OK;
				cancelColor = Color.V;
				Close();
			}).D(d);

			cancelButton.Events().Click
				.Subscribe(_ =>
				{
					DialogResult = DialogResult.Cancel;
					Close();
				}).D(d);
		});
	}

	private static void HookRgbPos(
		out IRoVar<RgbPos> rgbPos,
		RgbPickPanel rgbPickPanel,
		Disp d
	)
	{
		var rgbPosRw = Var.Make(RgbPos.Default, d);
		rgbPos = rgbPosRw;

		var serD = new SerialDisposable().D(d);

		void Lock()
		{
			serD.Disposable = null;
			User32Utils.SetCapture(rgbPickPanel.Handle);
			serD.Disposable = Disposable.Create(() => User32Utils.ReleaseCapture());
		}
		void Unlock() => serD.Disposable = null;


		var isPicking =
			Observable.Merge(
					rgbPickPanel.Events().MouseDown.Where(e => e.Button == MouseButtons.Left).Select(_ => true),
					rgbPickPanel.Events().MouseUp.Where(e => e.Button == MouseButtons.Left).Select(_ => false),
					rgbPickPanel.Events().MouseLeave.Select(_ => false)
				)
				.Prepend(false)
				.ToVar(d);



		var mousePos = Observable.Merge(
				rgbPickPanel.Events().MouseDown
					.Select(e => e.Location),
				rgbPickPanel.Events().MouseMove
					.Where(_ => isPicking.V)
					.Do(_ => Lock())
					.Select(e => e.Location)
			)
			.Prepend(Point.Empty)
			.ToVar(d);

		Observable.Merge(
				rgbPickPanel.Events().MouseUp.Where(e => e.Button == MouseButtons.Left).ToUnit(),
				rgbPickPanel.Events().MouseLeave.ToUnit()
		)
			.Where(_ => isPicking.V)
			.Subscribe(_ =>
			{
				Unlock();
			}).D(d);

		mousePos
			.Where(_ => isPicking.V)
			.Subscribe(pos =>
			{
				var val = rgbPickPanel.GetRgbPosAt(pos);
				rgbPosRw.V = val;
			}).D(d);
	}


	private static void HookHsv(
		out IRwVar<Hsv> hsv,
		TrackBar hueTrackBar,
		NumericUpDown hueUpDown,
		TrackBar satTrackBar,
		NumericUpDown satUpDown,
		TrackBar valTrackBar,
		NumericUpDown valUpDown,
		Disp d
	)
	{
		HookValue(out var hue, hueTrackBar, hueUpDown, d);
		HookValue(out var sat, satTrackBar, satUpDown, d);
		HookValue(out var val, valTrackBar, valUpDown, d);

		//var hsvRw = Var.Make(Hsv.Default).D(D);
		//hsv = hsvRw;
		//Observable.CombineLatest(hue, sat, val, (h, s, v) => new Hsv(h, s, v)).Subscribe(e => hsvRw.V = e).D(D);

		//var hsvRo = Var.BMerge(hue, sat, val, (h, s, v) => new Hsv(h, s, v)).D(D);

		hsv = Var.Make(Hsv.Default, d);
		var hsvLocal = hsv;

		Obs.CombineLatest(hue, sat, val, (h, s, v) => new Hsv(h, s, v)).Subscribe(e => hsvLocal.V = e).D(d);

		//Var.BMerge(hue, sat, val, (h, s, v) => new Hsv(h, s, v)).D(d)
		//	.Subscribe(hsvVal => hsvLocal.V = hsvVal).D(d);

		// HACK: StackOverflow
		//hsv.Subscribe(e =>
		//{
		//	hue.V = e.Hue;
		//	sat.V = e.Sat;
		//	val.V = e.Val;
		//}).D(d);
	}


	private static void HookValue(
		out IRwVar<int> val,
		TrackBar trackBar,
		NumericUpDown upDown,
		Disp d
	)
	{
		//var (valRw, _) = Var.Make(trackBar.Value).D(D);
		//val = valRw;
		var valRw = Var.Make(trackBar.Value, d);
		val = valRw;

		// HACK: StackOverflow
		//Observable.Merge(
		//		trackBar.Events().ValueChanged.Select(_ => trackBar.Value),
		//		upDown.Events().ValueChanged.Select(_ => (int)upDown.Value),
		//		val
		//	)
		//	.Subscribe(v =>
		//	{
		//		if (v != valRw.V) valRw.V = v;
		//		if (v != trackBar.Value) trackBar.Value = v;
		//		if (v != upDown.Value) upDown.Value = v;
		//	}).D(d);
	}
}
*/