using System.Reactive.Linq;
using ReactiveVars;

namespace SysWinInterfaces;


/// <summary>
/// The minimum amount of info needed from the window to initialize and resize DirectX
/// </summary>
public interface ISysWinRenderingSupport
{
	Disp D { get; }

	/// <summary>
	/// The window Handle. Assigned on WM_NCCREATE
	/// </summary>
	IntPtr Handle { get; }

	/// <summary>
	/// Have we received WM_CREATE ?
	/// </summary>
	IRoVar<bool> IsInit { get; }

	/// <summary>
	/// The client area
	/// </summary>
	IRoVar<Sz> ClientSz { get; }
}


public static class ISysWinRenderingSupportExts
{
	public static IObservable<Unit> WhenInit(this ISysWinRenderingSupport win) => win.IsInit.Where(e => e).ToUnit();
}
