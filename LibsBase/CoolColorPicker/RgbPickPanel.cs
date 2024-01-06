using System.Drawing.Imaging;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using CoolColorPicker.Structs;
using CoolColorPicker.Utils;
using ReactiveVars;
using UILib;

namespace CoolColorPicker;

sealed partial class RgbPickPanel : Panel
{
	private readonly ISubject<ColorUpdateEvt> whenUpdate;
	public IObservable<ColorUpdateEvt> WhenUpdate => whenUpdate.AsObservable();

	public void Set(ColorUpdateEvt evt)
	{
		curColor = evt.Color;
		var rgb = ToRgb(curColor);
		var closestPt = GetClosest(rgb, cursorPos.V);
		if (closestPt != cursorPos.V)
			cursorPos.V = closestPt;
	}


	private Rgba curColor = Consts.DefaultColor.ToRgba();


	private static readonly Bitmap colorSquareBmp = ColorPickerResources.rgb_square_2;
	private static readonly Bitmap crosshairBmp = ColorPickerResources.rgb_crosshair;
	private static readonly int crosshairWidth;
	private static readonly int crosshairHeight;
	private static readonly Rectangle imageRect;
	private static readonly Size imageSize;

	private record Rgb(int R, int G, int B);
	private record RgbPos(Rgb Rgb, Point Pos);
	private record Pt(Point Pos, Rgb Rgb);
	private static Rgb ToRgb(Rgba e) => new(e.R, e.G, e.B);


	private static readonly Pt[] data;
	private static readonly Dictionary<Point, Rgb> dataDict;

	private readonly IBoundVar<Point> cursorPos;


	static RgbPickPanel()
	{
		data = GetAllData(colorSquareBmp);
		dataDict = data.ToDictionary(e => e.Pos, e => e.Rgb);
		crosshairWidth = crosshairBmp.Width;
		crosshairHeight = crosshairBmp.Height;
		imageRect = new Rectangle(0, 0, colorSquareBmp.Width, colorSquareBmp.Height);
		imageSize = new Size(colorSquareBmp.Width, colorSquareBmp.Height);
	}

	public RgbPickPanel()
	{
		InitializeComponent();
		DoubleBuffered = true;

		MinimumSize = imageSize;
		MaximumSize = imageSize;
		Size = imageSize;

		var ctrlD = this.GetD();
		whenUpdate = new Subject<ColorUpdateEvt>().D(ctrlD);

		cursorPos = Var.MakeBound(Point.Empty, ctrlD);

		cursorPos.Subscribe(_ => Refresh()).D(ctrlD);

		this.InitRX(d =>
		{
			this.Events().Paint.Subscribe(e =>
			{
				var gfx = e.Graphics;
				gfx.DrawImage(colorSquareBmp, Point.Empty);
				var pos = cursorPos.V;
				var cPos = new Point(pos.X - crosshairWidth / 2, pos.Y - crosshairHeight / 2);
				gfx.DrawImage(crosshairBmp, cPos);
			}).D(d);



			cursorPos.WhenInner.Subscribe(e =>
			{
				var val = GetRgbPosAt(e);
				var evt = new ColorUpdateEvt(Src.RGBPanel, new Rgba(val.Rgb.R, val.Rgb.G, val.Rgb.B, curColor.A));
				whenUpdate.OnNext(evt);
			}).D(d);


			//var rgbPosRw = Var.Make(new RgbPos(ToRgb(Consts.DefaultColor.ToRgba()), Point.Empty), d);
			//rgbPos = rgbPosRw;

			var serD = new SerialDisposable().D(d);

			void Lock()
			{
				serD.Disposable = null;
				User32Utils.SetCapture(Handle);
				serD.Disposable = Disposable.Create(() => User32Utils.ReleaseCapture());
			}
			void Unlock() => serD.Disposable = null;


			var isPicking =
				Observable.Merge(
						this.Events().MouseDown.Where(e => e.Button == MouseButtons.Left).Select(_ => true),
						this.Events().MouseUp.Where(e => e.Button == MouseButtons.Left).Select(_ => false),
						this.Events().MouseLeave.Select(_ => false)
					)
					.Prepend(false)
					.ToVar(d);



			var mousePos = Observable.Merge(
					this.Events().MouseDown
						.Select(e => e.Location),
					this.Events().MouseMove
						.Where(_ => isPicking.V)
						.Do(_ => Lock())
						.Select(e => e.Location)
				)
				.Prepend(Point.Empty)
				.ToVar(d);

			Observable.Merge(
					this.Events().MouseUp.Where(e => e.Button == MouseButtons.Left).ToUnit(),
					this.Events().MouseLeave.ToUnit()
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
					cursorPos.SetInner(pos);
				}).D(d);
		});

	}



	private RgbPos GetRgbPosAt(Point pt)
	{
		var finalPt = CoordUtils.ProjectMouseOntoRectangle(pt, imageRect);
		var c = dataDict[finalPt];
		return new RgbPos(new Rgb(c.R, c.G, c.B), finalPt);
	}








	private static Point GetClosest(Rgb rgb, Point prevPos)
	{
		//var watch = Stopwatch.StartNew();

		var minPt = data[0];
		var minDist = int.MaxValue;
		var minDistPos = int.MaxValue;
		foreach (var pt in data)
		{
			var dist = Dist(pt.Rgb, rgb);
			if (dist < minDist)
			{
				minDist = dist;
				minPt = pt;
				minDistPos = PosDist(pt.Pos, prevPos);
			}

			if (dist == minDist)
			{
				var posDist = PosDist(pt.Pos, prevPos);
				if (posDist < minDistPos)
				{
					minDistPos = posDist;
					minPt = pt;
				}
			}
		}

		//Console.WriteLine($@"Time: {watch.ElapsedMilliseconds}ms");

		return minPt.Pos;
	}

	private static int Dist(Rgb a, Rgb b) =>
		(a.R - b.R) * (a.R - b.R) +
		(a.G - b.G) * (a.G - b.G) +
		(a.B - b.B) * (a.B - b.B);

	private static int PosDist(Point a, Point b) =>
		(a.X - b.X) * (a.X - b.X) +
		(a.Y - b.Y) * (a.Y - b.Y);

	private static Pt[] GetAllData(Bitmap bmp)
	{
		var rawData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
		var byteSize = rawData.Stride * bmp.Height;
		var byteData = new byte[byteSize];
		Marshal.Copy(rawData.Scan0, byteData, 0, byteSize);

		var tempData = new Pt[bmp.Width * bmp.Height];
		var ofsLine = 0;
		for (var y = 0; y < bmp.Height; y++)
		{
			var ofsPix = ofsLine;
			for (var x = 0; x < bmp.Width; x++)
			{
				var aVal = byteData[ofsPix + 3];
				var rVal = byteData[ofsPix + 2];
				var gVal = byteData[ofsPix + 1];
				var bVal = byteData[ofsPix + 0];
				var col = Color.FromArgb(aVal, rVal, gVal, bVal);

				var dataOfs = y * bmp.Width + x;
				var rgb = new Rgb(col.R, col.G, col.B);
				tempData[dataOfs] = new Pt(new Point(x, y), rgb);
				ofsPix += 4;
			}

			ofsLine += rawData.Stride;
		}

		bmp.UnlockBits(rawData);

		return tempData;
	}
}