<Query Kind="Program">
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Drawing.Drawing2D</Namespace>
</Query>

void Main()
{
	var folderIn = @"C:\dev\big\LinqVec\_infos\art\tool-icons";
	var zoom = 4;
	
	Util.VerticalRun(
		Directory.GetFiles(folderIn, "*.png")
			.Select(fileIn =>
				Util.HorizontalRun(true,
					Enum.GetValues<ToolIconState>()
						.Select(state => ToolIconUtils.Render(fileIn, state).Zoom(zoom))
						.ToArray()
				)
			)
			.ToArray()
	).Dump();
}


public enum ToolIconState
{
	Normal,
	Hover,
	MouseDown,
	Active
}

public static class ToolIconUtils
{
	private const int CornerRadius = 4;
	private static readonly Brush normalBack = new SolidBrush(MkCol(0x353535));
	private static readonly Brush hoverBack = new SolidBrush(MkCol(0x373737));
	private static readonly Pen hoverBorder = new Pen(MkCol(0x141414), 1.0f);
	private static readonly Brush mouseDownBack = new SolidBrush(MkCol(0x1E1E1E));
	private static readonly Pen mouseDownPen = new Pen(MkCol(0x808080), 1.0f)
	{
		DashStyle = DashStyle.Dot,
		DashOffset = 0.25f,
	};
	private static readonly Brush activeBack = new SolidBrush(MkCol(0x1F4A7D));
	private static readonly Pen activeBorder = new Pen(MkCol(0x141414), 1.0f);

	public static Bitmap Render(string fileIn, ToolIconState state)
	{
		var bmpIn = new Bitmap(fileIn);
		var bmpOut = new Bitmap(32, 32);
		using var gfx = Graphics.FromImage(bmpOut);
		var r = new Rectangle(0, 0, bmpOut.Width, bmpOut.Height);

		gfx.FillRectangle(normalBack, r);
		gfx.SmoothingMode = SmoothingMode.AntiAlias;

		switch (state)
		{
			case ToolIconState.Normal:
				gfx.DrawImage(bmpIn, 8, 8);
				break;

			case ToolIconState.Hover:
				gfx.FillRoundedRectangle(hoverBack, r.ShrinkSize(1, 1), CornerRadius);
				var hoverShadow = MakeHoverShadow(bmpIn, 180);
				gfx.DrawImage(hoverShadow, 8, 7.5f);
				gfx.DrawImage(bmpIn, 8, 8);
				gfx.SmoothingMode = SmoothingMode.AntiAlias;
				gfx.DrawRoundedRectangle(hoverBorder, r.ShrinkSize(1, 1), CornerRadius);
				break;

			case ToolIconState.MouseDown:
				gfx.FillRoundedRectangle(mouseDownBack, r.ShrinkSize(1, 1), CornerRadius);
				gfx.DrawRoundedRectangle(mouseDownPen, r.Shrink(2).ShrinkSize(1, 1), CornerRadius);
				gfx.DrawImage(bmpIn, 8, 8);
				break;
			
			case ToolIconState.Active:
				gfx.FillRoundedRectangle(activeBack, r.ShrinkSize(1, 1), CornerRadius);
				gfx.DrawRoundedRectangle(activeBorder, r.ShrinkSize(1, 1), CornerRadius);
				gfx.DrawImage(bmpIn, 8, 8);
				break;
		}

		return bmpOut;
	}
	
	private static RectangleF Ofs(this RectangleF r, float x, float y) => new(
		r.X + x,
		r.Y + y,
		r.Width,
		r.Height
	);
	private static RectangleF ShrinkSize(this RectangleF r, float x, float y) => new(
		r.X,
		r.Y,
		r.Width - x,
		r.Height - y
	);
	private static RectangleF ShrinkSize(this Rectangle r, float x, float y) => new(
		r.X,
		r.Y,
		r.Width - x,
		r.Height - y
	);

	public static Bitmap Zoom(this Bitmap bmpIn, int zoom)
	{
		var bmpOut = new Bitmap(bmpIn.Width * zoom, bmpIn.Height * zoom);
		using var gfx = Graphics.FromImage(bmpOut);
		gfx.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
		var rIn = new Rectangle(0, 0, bmpIn.Width, bmpIn.Height);
		var rOut = new Rectangle(0, 0, bmpOut.Width, bmpOut.Height);
		gfx.DrawImage(bmpIn, rOut, rIn, GraphicsUnit.Pixel);
		return bmpOut;
	}
	
	private static Bitmap MakeHoverShadow(Bitmap bmpIn, byte alpha)
	{
		var bmpOut = new Bitmap(bmpIn.Width, bmpIn.Height);
		for (var x = 0; x < bmpIn.Width; x++)
		for (var y = 0; y < bmpIn.Height; y++)
		{
			var cIn = bmpIn.GetPixel(x, y);
			var cOut = (cIn.A == 0) switch
			{
				true => cIn,
				false => Color.FromArgb(alpha, 0, 0, 0)
			};
			bmpOut.SetPixel(x, y, cOut);
		}
		return bmpOut;
	}


	private static void DrawRoundedRectangle(this Graphics graphics, Pen pen, RectangleF bounds, int cornerRadius)
	{
		if (graphics == null)
			throw new ArgumentNullException(nameof(graphics));
		if (pen == null)
			throw new ArgumentNullException(nameof(pen));

		using (GraphicsPath path = RoundedRect(bounds, cornerRadius))
		{
			graphics.DrawPath(pen, path);
		}
	}

	private static void FillRoundedRectangle(this Graphics graphics, Brush brush, RectangleF bounds, int cornerRadius)
	{
		if (graphics == null)
			throw new ArgumentNullException(nameof(graphics));
		if (brush == null)
			throw new ArgumentNullException(nameof(brush));

		using (GraphicsPath path = RoundedRect(bounds, cornerRadius))
		{
			graphics.FillPath(brush, path);
		}
	}

	private static GraphicsPath RoundedRect(RectangleF bounds, int radius)
	{
		int diameter = radius * 2;
		Size size = new Size(diameter, diameter);
		var arc = new RectangleF(bounds.Location, size);
		GraphicsPath path = new GraphicsPath();

		if (radius == 0)
		{
			path.AddRectangle(bounds);
			return path;
		}

		// top left arc  
		path.AddArc(arc, 180, 90);

		// top right arc  
		arc.X = (int)(bounds.Right - diameter);
		path.AddArc(arc, 270, 90);

		// bottom right arc  
		arc.Y = bounds.Bottom - diameter;
		path.AddArc(arc, 0, 90);

		// bottom left arc 
		arc.X = bounds.Left;
		path.AddArc(arc, 90, 90);

		path.CloseFigure();
		return path;
	}
	
	private static RectangleF Fit(this RectangleF r) => new(
		r.X,
		r.Y,
		r.Width - 1,
		r.Height - 1
	);
	private static RectangleF Shrink(this RectangleF r, int v) => new(
		r.X + v,
		r.Y + v,
		r.Width - 2 * v,
		r.Height - 2 * v
	);
	private static RectangleF Shrink(this Rectangle r, float v) => new(
		r.X + v,
		r.Y + v,
		r.Width - 2 * v,
		r.Height - 2 * v
	);
}



public static Color MkCol(uint v) => Color.FromArgb(0xFF, Color.FromArgb((int)v));
