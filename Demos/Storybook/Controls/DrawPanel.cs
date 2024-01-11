using System.ComponentModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using LinqVec.Utils;
using LogLib.Structs;
using ReactiveVars;
using Storybook.Logic;
using Storybook.Structs;
using Storybook.Utils;
using UILib;

namespace Storybook.Controls;

sealed partial class DrawPanel : UserControl
{
	private readonly ISubject<ColorClickedEvt> whenColorClicked;

	public PaletteKeeper PaletteKeeper { get; }
	public IRoVar<Option<TextChunk>> HoveredChunk { get; }
	public IObservable<ColorClickedEvt> WhenColorClicked => whenColorClicked.AsObservable();


	public DrawPanel()
	{
		DoubleBuffered = true;
		InitializeComponent();

		//File.WriteAllText(@"C:\tmp\log.txt", $"designLicense: {LicenseManager.UsageMode}\ndesign:{DesignMode}\nproc:{System.Diagnostics.Process.GetCurrentProcess().ProcessName}");

		if (WinFormsUtils.IsDesignMode) return;

		//var chunks = VecJsoner.Vec.Load<Txt>(Program.File);
		var chunks = Program.Chunks;
		var chunkMap = Painter.GetChunkMap(chunks);

		var ctrlD = this.GetD();
		PaletteKeeper = new PaletteKeeper(Program.CSharpColorFile, chunks, ctrlD);
		whenColorClicked = new Subject<ColorClickedEvt>().D(ctrlD);

		var hoveredChunk = Option<TextChunk>.None.Make(ctrlD);
		HoveredChunk = hoveredChunk;


		this.InitRX(d =>
		{
			PaletteKeeper.WhenPaintNeeded.Subscribe(_ =>
			{
				Refresh();
			}).D(d);

			this.Events().MouseMove.Subscribe(e =>
			{
				hoveredChunk.V = chunkMap.FirstOrOption(f => f.R.Contains(e.Location)).Map(f => f.Chunk);
			}).D(d);

			this.Events().MouseDown.Subscribe(e =>
			{
				ColType? colType = (e.Button.HasFlag(MouseButtons.Left), e.Button.HasFlag(MouseButtons.Right)) switch {
					(true, false) => ColType.Fore,
					(false, true) => ColType.Back,
					_ => null
				};
				if (colType == null) return;
				hoveredChunk.V.IfSome(chunk =>
				{
					switch (colType)
					{
						case ColType.Fore:
							chunk.Fore.IfSome(col => whenColorClicked.OnNext(new ColorClickedEvt(colType.Value, col)));
							break;
						case ColType.Back:
							chunk.Back.IfSome(col => whenColorClicked.OnNext(new ColorClickedEvt(colType.Value, col)));
							break;
						default:
							throw new ArgumentException();
					}
				});
			}).D(d);

			this.Events().Paint.Subscribe(e =>
			{
				var gfx = e.Graphics;
				var r = new Rectangle(0, 0, ClientSize.Width, ClientSize.Height);
				gfx.FillRectangle(Brushes.Black, r);

				Painter.PaintChunks(gfx, chunks, PaletteKeeper);

			}).D(d);
		});
	}
}
