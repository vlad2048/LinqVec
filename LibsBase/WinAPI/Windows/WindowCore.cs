using WinAPI.User32;

namespace WinAPI.Windows;

public struct WindowMessage
{
	public IntPtr Hwnd;
	public WM Id;
	public IntPtr WParam;
	public IntPtr LParam;
	public IntPtr Result;
	public bool Handled;

	public WindowMessage(IntPtr hwnd, WM id, IntPtr wParam, IntPtr lParam)
	{
		Hwnd = hwnd;
		Id = id;
		WParam = wParam;
		LParam = lParam;
		Result = IntPtr.Zero;
		Handled = false;
	}

	public void SetResult(IntPtr result)
	{
		Result = result;
	}

	public void SetResult(int result)
	{
		SetResult(new IntPtr(result));
	}

	public void MarkAsHandled() => Handled = true;
}
