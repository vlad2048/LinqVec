using System.Reactive.Linq;
using ReactiveVars;
using WinAPI.User32;
using WinAPI.Windows;

// ReSharper disable once CheckNamespace
namespace SysWinLib;

static class SysWinUtils
{
	public static void GenerateMouseLeaveMessagesIFN(this SysWin win, SysWinOpt opt)
	{
		if (!opt.GenerateMouseLeaveMessages) return;

		var isTracking =
			Obs.Merge(
					win.WhenMsg.WhenMOUSEMOVE().Select(_ => true),
					win.WhenMsg.WhenMOUSELEAVE().Select(_ => false)
				)
				.Prepend(false)
				.ToVar(win.D);

		isTracking
			.Where(e => e)
			.Where(_ => win.Handle != nint.Zero)
			.Subscribe(_ =>
			{
				User32Helpers.TrackMouseEventGenerateLeaveMessage(win.Handle);
			}).D(win.D);
	}
}