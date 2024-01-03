using WinAPI.Windows;

namespace SysWinInterfaces;


/// <summary>
/// The minimum amount of info needed from the window to derive user events from it
/// </summary>
public interface ISysWinUserEventsSupport
{
	IntPtr Handle { get; }
	IObservable<IPacket> WhenMsg { get; }
}