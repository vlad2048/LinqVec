using SysWinLib.Structs;
using WinAPI.User32;

// ReSharper disable once CheckNamespace
namespace SysWinLib;


public interface INCStrat
{
	public NCHitTestDelegate? HitTest { get; }
}

public static class NCStrats
{
	public static readonly INCStrat None = new NCStrat(null);

	public static INCStrat Always(HitTestResult res) => new NCStrat(
		(_, _) => res
	);

	public static INCStrat Popup(Func<Pt, bool> isOverMainWin) => new NCStrat(
		(_, pt) => isOverMainWin(pt) switch
		{
			true => HitTestResult.HTTRANSPARENT,
			false => HitTestResult.HTCLIENT
		}
	);
	
	public static INCStrat Custom(int gripSize = 7, int captionBarSize = 38) => new NCStrat(
		(winR, pt) =>
		{
			var sx = Math.Min(gripSize, winR.Width / 2);
			var sy = Math.Min(gripSize, winR.Height / 2);
			var left = pt.X - winR.X <= sx ? 1 : 0;
			var right = winR.Right - pt.X <= sx ? 1 : 0;
			var top = pt.Y - winR.Y <= sy ? 1 : 0;
			var bottom = winR.Bottom - pt.Y <= sy ? 1 : 0;
			var inCap = pt.Y - winR.Y + gripSize <= captionBarSize;

			var res = (left, right, top, bottom, inCap) switch
			{
				(1, 0, 1, 0, _) => HitTestResult.HTTOPLEFT,
				(0, 1, 1, 0, _) => HitTestResult.HTTOPRIGHT,
				(1, 0, 0, 1, _) => HitTestResult.HTBOTTOMLEFT,
				(0, 1, 0, 1, _) => HitTestResult.HTBOTTOMRIGHT,

				(1, 0, 0, 0, _) => HitTestResult.HTLEFT,
				(0, 1, 0, 0, _) => HitTestResult.HTRIGHT,
				(0, 0, 1, 0, _) => HitTestResult.HTTOP,
				(0, 0, 0, 1, _) => HitTestResult.HTBOTTOM,

				(0, 0, 0, 0, true) => HitTestResult.HTCAPTION,

				_ => HitTestResult.HTCLIENT,
			};

			return res;
		}
	);


	private sealed class NCStrat : INCStrat
	{
		public NCHitTestDelegate? HitTest { get; }

		public NCStrat(NCHitTestDelegate? hitTest)
		{
			HitTest = hitTest;
		}
	}
}
