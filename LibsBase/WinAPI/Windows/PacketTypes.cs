using System.Runtime.InteropServices;
using WinAPI.NetCoreEx.BinaryExtensions;
using WinAPI.NetCoreEx.Geometry;
using WinAPI.User32;

namespace WinAPI.Windows;

public interface IPacket
{
	WM MsgId { get; }
	void MarkAsHandled();
}

public struct Packet : IPacket
{
	public unsafe WM MsgId => Message->Id;
	public unsafe void MarkAsHandled() => Message->MarkAsHandled();

	public unsafe WindowMessage* Message;

	public unsafe Packet(WindowMessage* message)
	{
		Message = message;
	}

	public unsafe IntPtr Hwnd
	{
		get => Message->Hwnd;
		set => Message->Hwnd = value;
	}
}


public struct SizePacket : IPacket
{
	public unsafe WM MsgId => Message->Id;
	public unsafe void MarkAsHandled() => Message->MarkAsHandled();

	public unsafe WindowMessage* Message;

	public unsafe SizePacket(WindowMessage* message)
	{
		Message = message;
	}

	public unsafe IntPtr Hwnd
	{
		get => Message->Hwnd;
		set => Message->Hwnd = value;
	}

	public unsafe WindowSizeFlag Flag
	{
		get => (WindowSizeFlag)Message->WParam.ToSafeInt32();
		set => Message->WParam = new IntPtr((int)value);
	}

	public unsafe Size Size
	{
		get
		{
			Size size;
			Message->LParam.BreakSafeInt32To16Signed(out size.Height, out size.Width);
			return size;
		}

		set => Message->LParam = new IntPtr(value.ToInt32());
	}
}

public struct MovePacket : IPacket
{
	public unsafe WM MsgId => Message->Id;
	public unsafe void MarkAsHandled() => Message->MarkAsHandled();

	public unsafe WindowMessage* Message;

	public unsafe MovePacket(WindowMessage* message)
	{
		Message = message;
	}

	public unsafe IntPtr Hwnd
	{
		get => Message->Hwnd;
		set => Message->Hwnd = value;
	}

	public unsafe Point Point
	{
		get
		{
			Point point;
			Message->LParam.BreakSafeInt32To16Signed(out point.Y, out point.X);
			return point;
		}

		set => Message->LParam = new IntPtr(value.ToInt32());
	}
}

public struct PaintPacket : IPacket
{
	public unsafe WM MsgId => Message->Id;
	public unsafe void MarkAsHandled() => Message->MarkAsHandled();

	public unsafe WindowMessage* Message;

	public unsafe PaintPacket(WindowMessage* message)
	{
		Message = message;
	}

	public unsafe IntPtr Hwnd
	{
		get => Message->Hwnd;
		set => Message->Hwnd = value;
	}

	public unsafe IntPtr Hdc
	{
		get => Message->WParam;
		set => Message->WParam = value;
	}
}

public struct MinMaxInfoPacket : IPacket
{
	public unsafe WM MsgId => Message->Id;
	public unsafe void MarkAsHandled() => Message->MarkAsHandled();

	public unsafe WindowMessage* Message;

	public unsafe MinMaxInfoPacket(WindowMessage* message)
	{
		Message = message;
	}

	public unsafe IntPtr Hwnd
	{
		get => Message->Hwnd;
		set => Message->Hwnd = value;
	}

	public unsafe ref MinMaxInfo Info => ref ((MinMaxInfoWrapper*)Message->LParam)->Value;

}

public struct EraseBkgndPacket : IPacket
{
	public unsafe WM MsgId => Message->Id;
	public unsafe void MarkAsHandled() => Message->MarkAsHandled();

	public unsafe WindowMessage* Message;

	public unsafe EraseBkgndPacket(WindowMessage* message)
	{
		Message = message;
	}

	public unsafe IntPtr Hwnd
	{
		get => Message->Hwnd;
		set => Message->Hwnd = value;
	}

	public unsafe IntPtr Hdc
	{
		get => Message->WParam;
		set => Message->WParam = value;
	}

	public unsafe EraseBackgroundResult Result
	{
		get => (EraseBackgroundResult)Message->Result.ToSafeInt32();
		set => Message->Result = new IntPtr((int)value);
	}
}

public struct WindowPositionPacket : IPacket
{
	public unsafe WM MsgId => Message->Id;
	public unsafe void MarkAsHandled() => Message->MarkAsHandled();

	public unsafe WindowMessage* Message;

	public unsafe WindowPositionPacket(WindowMessage* message)
	{
		Message = message;
	}

	public unsafe IntPtr Hwnd
	{
		get => Message->Hwnd;
		set => Message->Hwnd = value;
	}

	public unsafe ref WindowPosition Position => ref ((WindowPositionWrapper*)Message->LParam)->Value;

}

public struct ShowWindowPacket : IPacket
{
	public unsafe WM MsgId => Message->Id;
	public unsafe void MarkAsHandled() => Message->MarkAsHandled();

	public unsafe WindowMessage* Message;

	public unsafe ShowWindowPacket(WindowMessage* message)
	{
		Message = message;
	}

	public unsafe IntPtr Hwnd
	{
		get => Message->Hwnd;
		set => Message->Hwnd = value;
	}

	public unsafe ShowWindowStatusFlags Flags
	{
		get => (ShowWindowStatusFlags)Message->LParam.ToSafeInt32();
		set => Message->LParam = new IntPtr((int)value);
	}

	public unsafe bool IsShown
	{
		get => Message->WParam.ToSafeInt32() > 0;
		set => Message->WParam = value ? 1 : 0;
	}
}

public struct QuitPacket : IPacket
{
	public unsafe WM MsgId => Message->Id;
	public unsafe void MarkAsHandled() => Message->MarkAsHandled();

	public unsafe WindowMessage* Message;

	public unsafe QuitPacket(WindowMessage* message)
	{
		Message = message;
	}

	public unsafe IntPtr Hwnd
	{
		get => Message->Hwnd;
		set => Message->Hwnd = value;
	}

	public unsafe int Code
	{
		get => Message->WParam.ToSafeInt32();
		set => Message->WParam = value;
	}
}

public struct CreateWindowPacket : IPacket
{
	public unsafe WM MsgId => Message->Id;
	public unsafe void MarkAsHandled() => Message->MarkAsHandled();

	public unsafe WindowMessage* Message;

	public unsafe CreateWindowPacket(WindowMessage* message)
	{
		Message = message;
	}

	public unsafe IntPtr Hwnd
	{
		get => Message->Hwnd;
		set => Message->Hwnd = value;
	}

	public unsafe ref CreateStruct CreateStruct => ref ((CreateStructWrapper*)Message->LParam)->Value;


	// Return 0 to continue creation. -1 to destroy and prevent
	public unsafe CreateWindowResult Result
	{
		get => (CreateWindowResult)Message->Result.ToSafeInt32();
		set => Message->Result = new IntPtr((int)value);
	}
}

public struct ActivatePacket : IPacket
{
	public unsafe WM MsgId => Message->Id;
	public unsafe void MarkAsHandled() => Message->MarkAsHandled();

	public unsafe WindowMessage* Message;

	public unsafe ActivatePacket(WindowMessage* message)
	{
		Message = message;
	}

	public unsafe IntPtr Hwnd
	{
		get => Message->Hwnd;
		set => Message->Hwnd = value;
	}

	public unsafe IntPtr ConverseHwnd
	{
		get => Message->LParam;
		set => Message->LParam = value;
	}

	public unsafe bool IsMinimized
	{
		get => GetWParamAsInt().High() != 0;
		set => Message->WParam = new IntPtr(GetWParamAsInt().WithHigh((short)(value ? 1 : 0)));
	}

	public unsafe WindowActivateFlag Flag
	{
		get => (WindowActivateFlag)GetWParamAsInt().LowAsInt();
		set => Message->WParam = new IntPtr(GetWParamAsInt().WithLow(unchecked((short)value)));
	}


	private unsafe int GetWParamAsInt()
	{
		return Message->WParam.ToSafeInt32();
	}
}

public struct ActivateAppPacket : IPacket
{
	public unsafe WM MsgId => Message->Id;
	public unsafe void MarkAsHandled() => Message->MarkAsHandled();

	public unsafe WindowMessage* Message;

	public unsafe IntPtr Hwnd
	{
		get => Message->Hwnd;
		set => Message->Hwnd = value;
	}

	public unsafe ActivateAppPacket(WindowMessage* message)
	{
		Message = message;
	}

	public unsafe uint ConverseThreadId
	{
		get => Message->LParam.ToSafeUInt32();
		set => Message->LParam = new IntPtr(unchecked((int)value));
	}

	public unsafe bool IsActive
	{
		get => Message->WParam.ToSafeInt32() != 0;
		set => Message->WParam = new IntPtr(value ? 1 : 0);
	}
}

public struct KeyCharPacket : IPacket
{
	public unsafe WM MsgId => Message->Id;
	public unsafe void MarkAsHandled() => Message->MarkAsHandled();

	public unsafe WindowMessage* Message;

	public unsafe KeyCharPacket(WindowMessage* message)
	{
		Message = message;
	}

	public unsafe IntPtr Hwnd
	{
		get => Message->Hwnd;
		set => Message->Hwnd = value;
	}

	public unsafe char Key
	{
		get => (char)Message->WParam.ToSafeInt32();
		set => Message->WParam = new IntPtr(value);
	}

	public unsafe KeyboardInputState InputState
	{
		get => new(Message->LParam.ToSafeUInt32());
		set => Message->LParam = new IntPtr(unchecked((int)value.Value));
	}

	public unsafe bool IsDeadChar
	{
		get
		{
			var id = Message->Id;
			return id == WM.DEADCHAR || id == WM.SYSDEADCHAR;
		}
		set =>
			Message->Id = value
				? IsSystemContext ? WM.SYSDEADCHAR : WM.DEADCHAR
				: IsSystemContext ? WM.SYSCHAR : WM.CHAR;
	}

	public unsafe bool IsSystemContext
	{
		get
		{
			var id = Message->Id;
			return id == WM.SYSCHAR || id == WM.SYSDEADCHAR;
		}
		set =>
			Message->Id = value
				? IsDeadChar ? WM.SYSDEADCHAR : WM.SYSCHAR
				: IsDeadChar ? WM.DEADCHAR : WM.CHAR;
	}
}

public struct KeyPacket : IPacket
{
	public unsafe WM MsgId => Message->Id;
	public unsafe void MarkAsHandled() => Message->MarkAsHandled();

	public unsafe WindowMessage* Message;

	public unsafe KeyPacket(WindowMessage* message)
	{
		Message = message;
	}

	public unsafe IntPtr Hwnd
	{
		get => Message->Hwnd;
		set => Message->Hwnd = value;
	}

	public unsafe VirtualKey Key
	{
		get => (VirtualKey)Message->WParam.ToSafeInt32();
		set => Message->WParam = new IntPtr((int)value);
	}

	public unsafe KeyboardInputState InputState
	{
		get => new(Message->LParam.ToSafeUInt32());
		set => Message->LParam = new IntPtr(unchecked((int)value.Value));
	}

	public unsafe bool IsKeyDown
	{
		get
		{
			var id = Message->Id;
			return id == WM.KEYDOWN || id == WM.SYSKEYDOWN;
		}
		set =>
			Message->Id = value
				? IsSystemContext ? WM.SYSKEYDOWN : WM.KEYDOWN
				: IsSystemContext ? WM.SYSKEYUP : WM.KEYUP;
	}

	public unsafe bool IsSystemContext
	{
		get
		{
			var id = Message->Id;
			return id == WM.SYSKEYDOWN || id == WM.SYSKEYUP;
		}
		set =>
			Message->Id = value
				? IsKeyDown ? WM.SYSKEYDOWN : WM.SYSKEYUP
				: IsKeyDown ? WM.KEYDOWN : WM.KEYUP;
	}
}

public struct MousePacket : IPacket
{
	public unsafe WM MsgId => Message->Id;
	public unsafe void MarkAsHandled() => Message->MarkAsHandled();

	public unsafe WindowMessage* Message;

	public unsafe MousePacket(WindowMessage* message)
	{
		Message = message;
	}

	public unsafe IntPtr Hwnd
	{
		get => Message->Hwnd;
		set => Message->Hwnd = value;
	}

	public unsafe Point Point
	{
		get
		{
			Point point;
			Message->LParam.BreakSafeInt32To16Signed(out point.Y, out point.X);
			return point;
		}

		set => Message->LParam = new IntPtr(value.ToInt32());
	}

	public unsafe MouseInputKeyStateFlags InputState
	{
		get => (MouseInputKeyStateFlags)GetWParamAsInt();
		set => Message->WParam = new IntPtr((int)value);
	}

	private unsafe int GetWParamAsInt()
	{
		return Message->WParam.ToSafeInt32();
	}
}

public struct MouseButtonPacket : IPacket
{
	public unsafe WM MsgId => Message->Id;
	public unsafe void MarkAsHandled() => Message->MarkAsHandled();

	public unsafe WindowMessage* Message;

	public unsafe IntPtr Hwnd
	{
		get => Message->Hwnd;
		set => Message->Hwnd = value;
	}

	public unsafe MouseButtonPacket(WindowMessage* message)
	{
		Message = message;
	}

	private unsafe int GetWParamAsInt()
	{
		return Message->WParam.ToSafeInt32();
	}

	public unsafe Point Point
	{
		get
		{
			Point point;
			Message->LParam.BreakSafeInt32To16Signed(out point.Y, out point.X);
			return point;
		}

		set => Message->LParam = new IntPtr(value.ToInt32());
	}

	public MouseButton Button
	{
		get => GetButton();
		set => SetButton(value);
	}

	public unsafe bool IsButtonDown
	{
		get
		{
			var id = Message->Id;
			// Unfortunately, there's no better way than to do a full check here, since the numerical
			// values don't have any valid pattern to do it in one-go.
			return id == WM.LBUTTONDOWN || id == WM.RBUTTONDOWN || id == WM.MBUTTONDOWN ||
			       id == WM.XBUTTONDOWN;
		}

		set
		{
			var button = GetButton();
			switch (button)
			{
				case MouseButton.Left:
					Message->Id = value ? WM.LBUTTONDOWN : WM.LBUTTONUP;
					return;
				case MouseButton.Right:
					Message->Id = value ? WM.RBUTTONDOWN : WM.RBUTTONUP;
					return;
				case MouseButton.Middle:
					Message->Id = value ? WM.MBUTTONDOWN : WM.MBUTTONUP;
					return;
				default:
					Message->Id = value ? WM.XBUTTONDOWN : WM.XBUTTONUP;
					return;
			}
		}
	}

	public unsafe MouseInputKeyStateFlags InputState
	{
		get => (MouseInputKeyStateFlags)GetWParamAsInt().LowAsInt();
		set => Message->WParam = new IntPtr(GetWParamAsInt().WithLow(unchecked((short)value)));
	}

	public unsafe MouseButtonResult Result
	{
		get => (MouseButtonResult)Message->Result;
		set => Message->Result = new IntPtr((int)value);
	}

	private unsafe void SetButton(MouseButton value)
	{
		switch (value)
		{
			case MouseButton.Left:
				Message->Id = IsButtonDown ? WM.LBUTTONDOWN : WM.LBUTTONUP;
				return;
			case MouseButton.Right:
				Message->Id = IsButtonDown ? WM.RBUTTONDOWN : WM.RBUTTONUP;
				return;
			case MouseButton.Middle:
				Message->Id = IsButtonDown ? WM.MBUTTONDOWN : WM.MBUTTONUP;
				return;
			default:
				Message->Id = IsButtonDown ? WM.XBUTTONDOWN : WM.XBUTTONUP;
				Message->WParam = new IntPtr(GetWParamAsInt().WithHigh(unchecked((short)value)));
				return;
		}
	}

	// ReSharper disable PatternIsRedundant
	private unsafe MouseButton GetButton()
	{
		var id = (int)Message->Id;
		return id switch
		{
			// Unfortunately, there's no better way than to do a full check here, since the numerical
			// values don't have any valid pattern to do it in one-go.
			> 0x200 and < 0x204 => MouseButton.Left,
			> 0x203 and < 0x207 => MouseButton.Right,
			> 0x206 and <= 0x209 => MouseButton.Middle,
			_ => (MouseInputXButtonFlag)GetWParamAsInt().HighAsInt() == MouseInputXButtonFlag.XBUTTON1 ? MouseButton.XButton1 : MouseButton.XButton2
		};
	}
	// ReSharper restore PatternIsRedundant
}

public struct MouseDoubleClickPacket : IPacket
{
	public unsafe WM MsgId => Message->Id;
	public unsafe void MarkAsHandled() => Message->MarkAsHandled();

	public unsafe WindowMessage* Message;

	public unsafe MouseDoubleClickPacket(WindowMessage* message)
	{
		Message = message;
	}

	public unsafe IntPtr Hwnd
	{
		get => Message->Hwnd;
		set => Message->Hwnd = value;
	}

	private unsafe int GetWParamAsInt()
	{
		return Message->WParam.ToSafeInt32();
	}

	public unsafe Point Point
	{
		get
		{
			Point point;
			Message->LParam.BreakSafeInt32To16Signed(out point.Y, out point.X);
			return point;
		}

		set => Message->LParam = new IntPtr(value.ToInt32());
	}

	public MouseButton Button
	{
		get => GetButton();
		set => SetButton(value);
	}

	public unsafe MouseInputKeyStateFlags InputState
	{
		get => (MouseInputKeyStateFlags)GetWParamAsInt().LowAsInt();
		set => Message->WParam = new IntPtr(GetWParamAsInt().WithLow(unchecked((short)value)));
	}

	public unsafe MouseButtonResult Result
	{
		get => (MouseButtonResult)Message->Result;
		set => Message->Result = new IntPtr((int)value);
	}

	private unsafe void SetButton(MouseButton value)
	{
		switch (value)
		{
			case MouseButton.Left:
				Message->Id = WM.LBUTTONDBLCLK;
				return;
			case MouseButton.Right:
				Message->Id = WM.RBUTTONDBLCLK;
				return;
			case MouseButton.Middle:
				Message->Id = WM.MBUTTONDBLCLK;
				return;
			default:
				Message->Id = WM.XBUTTONDBLCLK;
				Message->WParam = new IntPtr(GetWParamAsInt().WithHigh(unchecked((short)value)));
				return;
		}
	}

	private unsafe MouseButton GetButton()
	{
		var id = Message->Id;
		switch (id)
		{
			case WM.LBUTTONDBLCLK:
				return MouseButton.Left;
			case WM.RBUTTONDBLCLK:
				return MouseButton.Right;
			case WM.MBUTTONDBLCLK:
				return MouseButton.Middle;
			default:
			{
				return (MouseInputXButtonFlag)GetWParamAsInt().HighAsInt()
				       == MouseInputXButtonFlag.XBUTTON1
					? MouseButton.XButton1
					: MouseButton.XButton2;
			}
		}
	}
}

public struct MouseWheelPacket : IPacket
{
	public unsafe WM MsgId => Message->Id;
	public unsafe void MarkAsHandled() => Message->MarkAsHandled();

	public unsafe WindowMessage* Message;

	public unsafe MouseWheelPacket(WindowMessage* message)
	{
		Message = message;
	}

	public unsafe IntPtr Hwnd
	{
		get => Message->Hwnd;
		set => Message->Hwnd = value;
	}

	private unsafe int GetWParamAsInt()
	{
		return Message->WParam.ToSafeInt32();
	}

	public unsafe Point Point
	{
		get
		{
			Point point;
			Message->LParam.BreakSafeInt32To16Signed(out point.Y, out point.X);
			return point;
		}

		set => Message->LParam = new IntPtr(value.ToInt32());
	}

	public unsafe MouseInputKeyStateFlags InputState
	{
		get => (MouseInputKeyStateFlags)GetWParamAsInt().LowAsInt();
		set => Message->WParam = new IntPtr(GetWParamAsInt().WithLow(unchecked((short)value)));
	}

	// Multiple or divisons of (WHEEL_DELTA = 120)
	public unsafe short WheelDelta
	{
		get => GetWParamAsInt().High();
		set => Message->WParam = new IntPtr(GetWParamAsInt().WithHigh(value));
	}

	public unsafe bool IsVertical
	{
		get => Message->Id == WM.MOUSEWHEEL;
		set => Message->Id = value ? WM.MOUSEWHEEL : WM.MOUSEHWHEEL;
	}
}

public struct MouseActivatePacket : IPacket
{
	public unsafe WM MsgId => Message->Id;
	public unsafe void MarkAsHandled() => Message->MarkAsHandled();

	public unsafe WindowMessage* Message;

	public unsafe MouseActivatePacket(WindowMessage* message)
	{
		Message = message;
	}

	public unsafe IntPtr Hwnd
	{
		get => Message->Hwnd;
		set => Message->Hwnd = value;
	}

	private unsafe int GetLParamAsInt()
	{
		return Message->LParam.ToSafeInt32();
	}

	public unsafe IntPtr TopLevelActiveParentHwnd
	{
		get => Message->WParam;
		set => Message->WParam = value;
	}

	public unsafe HitTestResult HitTestResult
	{
		get => (HitTestResult)GetLParamAsInt().LowAsInt();
		set => Message->LParam = new IntPtr(GetLParamAsInt().WithLow(unchecked((short)value)));
	}

	public unsafe ushort MouseMessageId
	{
		get => (ushort)GetLParamAsInt().High();
		set => Message->LParam = new IntPtr(GetLParamAsInt().WithHigh(unchecked((short)value)));
	}

	public unsafe MouseActivationResult Result
	{
		get => (MouseActivationResult)Message->Result;
		set => Message->Result = new IntPtr((int)value);
	}
}

public struct DisplayChangePacket : IPacket
{
	public unsafe WM MsgId => Message->Id;
	public unsafe void MarkAsHandled() => Message->MarkAsHandled();

	public unsafe WindowMessage* Message;

	public unsafe DisplayChangePacket(WindowMessage* message)
	{
		Message = message;
	}

	public unsafe IntPtr Hwnd
	{
		get => Message->Hwnd;
		set => Message->Hwnd = value;
	}

	public unsafe uint ImageDepthAsBitsPerPixel
	{
		get => (uint)Message->WParam.ToPointer();
		set => Message->WParam = new IntPtr(unchecked((int)value));
	}

	public unsafe Size Size
	{
		get
		{
			Size size;
			Message->LParam.BreakSafeInt32To16Signed(out size.Height, out size.Width);
			return size;
		}

		set => Message->LParam = new IntPtr(value.ToInt32());
	}
}

public struct CommandPacket : IPacket
{
	public unsafe WM MsgId => Message->Id;
	public unsafe void MarkAsHandled() => Message->MarkAsHandled();

	public unsafe WindowMessage* Message;

	public unsafe CommandPacket(WindowMessage* message)
	{
		Message = message;
	}

	public unsafe IntPtr Hwnd
	{
		get => Message->Hwnd;
		set => Message->Hwnd = value;
	}

	public unsafe IntPtr CommandHwnd
	{
		get => Message->LParam;
		set => Message->LParam = value;
	}

	public unsafe short Id
	{
		get => GetWParamAsInt().Low();
		set => Message->WParam = new IntPtr(GetWParamAsInt().WithLow(value));
	}

	public unsafe CommandSource CommandSource
	{
		get => (CommandSource)GetWParamAsInt().HighAsInt();
		set => Message->WParam = new IntPtr(GetWParamAsInt().WithHigh(unchecked((short)value)));
	}

	private unsafe int GetWParamAsInt()
	{
		return Message->WParam.ToSafeInt32();
	}
}

public struct SysCommandPacket : IPacket
{
	public unsafe WM MsgId => Message->Id;
	public unsafe void MarkAsHandled() => Message->MarkAsHandled();

	public unsafe WindowMessage* Message;

	public unsafe SysCommandPacket(WindowMessage* message)
	{
		Message = message;
	}

	public unsafe IntPtr Hwnd
	{
		get => Message->Hwnd;
		set => Message->Hwnd = value;
	}

	public unsafe short X
	{
		get => GetLParamAsInt().Low();
		set => Message->LParam = new IntPtr(GetLParamAsInt().WithLow(value));
	}

	public unsafe short Y
	{
		get => GetLParamAsInt().High();
		set => Message->LParam = new IntPtr(GetLParamAsInt().WithHigh(value));
	}

	public unsafe SysCommand Command
	{
		get => (SysCommand)Message->WParam.ToSafeInt32();
		set => Message->WParam = new IntPtr((int)value);
	}

	public bool IsAccelerator
	{
		get => Y == -1;
		set
		{
			if (value) Y = -1;
		}
	}

	public unsafe bool IsMnemonic
	{
		get => GetLParamAsInt() == 0;
		set
		{
			if (value) Message->LParam = IntPtr.Zero;
		}
	}

	private unsafe int GetLParamAsInt()
	{
		return Message->LParam.ToSafeInt32();
	}
}

public struct MenuCommandPacket : IPacket
{
	public unsafe WM MsgId => Message->Id;
	public unsafe void MarkAsHandled() => Message->MarkAsHandled();

	public unsafe WindowMessage* Message;

	public unsafe MenuCommandPacket(WindowMessage* message)
	{
		Message = message;
	}

	public unsafe IntPtr Hwnd
	{
		get => Message->Hwnd;
		set => Message->Hwnd = value;
	}

	public unsafe int MenuIndex
	{
		get => Message->WParam.ToSafeInt32();
		set => Message->WParam = new IntPtr(value);
	}

	public unsafe IntPtr MenuHandle
	{
		get => Message->LParam;
		set => Message->LParam = value;
	}
}

public struct AppCommandPacket : IPacket
{
	public unsafe WM MsgId => Message->Id;
	public unsafe void MarkAsHandled() => Message->MarkAsHandled();

	public unsafe WindowMessage* Message;

	public unsafe AppCommandPacket(WindowMessage* message)
	{
		Message = message;
	}

	public unsafe IntPtr Hwnd
	{
		get => Message->Hwnd;
		set => Message->Hwnd = value;
	}

	public unsafe IntPtr CommandHwnd
	{
		get => Message->WParam;
		set => Message->WParam = value;
	}

	public unsafe KeyboardInputState InputState
	{
		get => new((uint)GetLParamAsInt().LowAsInt());
		set => Message->LParam = new IntPtr(GetLParamAsInt().WithLow(unchecked((short)value.Value)));
	}

	public unsafe AppCommand Command
	{
		//GET_APPCOMMAND_LPARAM(lParam) ((short)(HIWORD(lParam) & ~FAPPCOMMAND_MASK))
		get => (AppCommand)(GetLParamAsInt().HighAsInt() & ~(uint)AppCommandDevice.FAPPCOMMAND_MASK);
		set
		{
			var high16 = GetLParamAsInt().High();
			var mask = ~(uint)AppCommandDevice.FAPPCOMMAND_MASK;
			var final = (high16 & ~mask) | ((int)value & mask);
			Message->LParam = new IntPtr(unchecked((int)final));
		}
	}

	public unsafe AppCommandDevice Device
	{
		//GET_DEVICE_LPARAM(lParam)     ((WORD)(HIWORD(lParam) & FAPPCOMMAND_MASK))
		get => (AppCommandDevice)(GetLParamAsInt().HighAsInt() & (uint)AppCommandDevice.FAPPCOMMAND_MASK);
		set
		{
			var high16 = GetLParamAsInt().High();
			var mask = (uint)AppCommandDevice.FAPPCOMMAND_MASK;
			var final = (high16 & ~mask) | ((int)value & mask);
			Message->LParam = new IntPtr(unchecked((int)final));
		}
	}

	public unsafe AppCommandResult Result
	{
		get => (AppCommandResult)Message->Result;
		set => Message->Result = new IntPtr((int)value);
	}

	private unsafe int GetLParamAsInt()
	{
		return Message->LParam.ToSafeInt32();
	}
}

public struct CaptureChangedPacket : IPacket
{
	public unsafe WM MsgId => Message->Id;
	public unsafe void MarkAsHandled() => Message->MarkAsHandled();

	public unsafe WindowMessage* Message;

	public unsafe CaptureChangedPacket(WindowMessage* message)
	{
		Message = message;
	}

	public unsafe IntPtr Hwnd
	{
		get => Message->Hwnd;
		set => Message->Hwnd = value;
	}

	public unsafe IntPtr CapturingHwnd
	{
		get => Message->LParam;
		set => Message->LParam = value;
	}
}

public struct FocusPacket : IPacket
{
	public unsafe WM MsgId => Message->Id;
	public unsafe void MarkAsHandled() => Message->MarkAsHandled();

	public unsafe WindowMessage* Message;

	public unsafe FocusPacket(WindowMessage* message)
	{
		Message = message;
	}

	public unsafe IntPtr Hwnd
	{
		get => Message->Hwnd;
		set => Message->Hwnd = value;
	}

	public unsafe IntPtr ConverseHwnd
	{
		get => Message->WParam;
		set => Message->WParam = value;
	}
}

public struct HotKeyPacket : IPacket
{
	public unsafe WM MsgId => Message->Id;
	public unsafe void MarkAsHandled() => Message->MarkAsHandled();

	public unsafe WindowMessage* Message;

	public unsafe HotKeyPacket(WindowMessage* message)
	{
		Message = message;
	}

	public unsafe IntPtr Hwnd
	{
		get => Message->Hwnd;
		set => Message->Hwnd = value;
	}

	private unsafe int GetLParamAsInt()
	{
		return Message->LParam.ToSafeInt32();
	}

	public unsafe ScreenshotHotKey ScreenshotHotKey
	{
		get => (ScreenshotHotKey)Message->WParam.ToSafeInt32();
		set => Message->WParam = new IntPtr((int)value);
	}

	public unsafe HotKeyInputState KeyState
	{
		get => (HotKeyInputState)GetLParamAsInt().Low();
		set => Message->LParam = new IntPtr(GetLParamAsInt().WithLow(unchecked((short)value)));
	}

	public unsafe VirtualKey Key
	{
		get => (VirtualKey)GetLParamAsInt().High();
		set => Message->LParam = new IntPtr(GetLParamAsInt().WithHigh(unchecked((short)value)));
	}
}

public struct NcHitTestPacket : IPacket
{
	public unsafe WM MsgId => Message->Id;
	public unsafe void MarkAsHandled() => Message->MarkAsHandled();

	public unsafe WindowMessage* Message;

	public unsafe NcHitTestPacket(WindowMessage* message)
	{
		Message = message;
	}

	public unsafe IntPtr Hwnd
	{
		get => Message->Hwnd;
		set => Message->Hwnd = value;
	}

	public unsafe Point Point
	{
		get
		{
			Point point;
			Message->LParam.BreakSafeInt32To16Signed(out point.Y, out point.X);
			return point;
		}

		set => Message->LParam = new IntPtr(value.ToInt32());
	}

	public unsafe HitTestResult Result
	{
		get => (HitTestResult)Message->Result;
		set => Message->Result = new IntPtr((int)value);
	}
}

public struct NcPaintPacket : IPacket
{
	public unsafe WM MsgId => Message->Id;
	public unsafe void MarkAsHandled() => Message->MarkAsHandled();

	public unsafe WindowMessage* Message;

	public unsafe NcPaintPacket(WindowMessage* message)
	{
		Message = message;
	}

	public unsafe IntPtr Hwnd
	{
		get => Message->Hwnd;
		set => Message->Hwnd = value;
	}

	public unsafe IntPtr UpdateRegion
	{
		get => Message->WParam;
		set => Message->WParam = value;
	}
}

public struct NcMouseMovePacket : IPacket
{
	public unsafe WM MsgId => Message->Id;
	public unsafe void MarkAsHandled() => Message->MarkAsHandled();

	public unsafe WindowMessage* Message;

	public unsafe NcMouseMovePacket(WindowMessage* message)
	{
		Message = message;
	}

	public unsafe IntPtr Hwnd
	{
		get => Message->Hwnd;
		set => Message->Hwnd = value;
	}

	public unsafe HitTestResult HitTestValue
	{
		get => (HitTestResult)Message->WParam;
		set => Message->WParam = new IntPtr((int)value);
	}

	public unsafe Point Point
	{
		get
		{
			Point point;
			Message->LParam.BreakSafeInt32To16Signed(out point.Y, out point.X);
			return point;
		}

		set => Message->LParam = new IntPtr(value.ToInt32());
	}
}

public struct NcActivatePacket : IPacket
{
	public unsafe WM MsgId => Message->Id;
	public unsafe void MarkAsHandled() => Message->MarkAsHandled();

	public unsafe WindowMessage* Message;

	public unsafe NcActivatePacket(WindowMessage* message)
	{
		Message = message;
	}

	public unsafe IntPtr Hwnd
	{
		get => Message->Hwnd;
		set => Message->Hwnd = value;
	}

	public unsafe bool IsActive
	{
		get => Message->WParam.ToSafeInt32() > 0;
		set => Message->WParam = (value ? 1 : 0);
	}

	// lParam is used only if visual styles are disabled
	public unsafe IntPtr UpdateRegion
	{
		get => Message->LParam;
		set => Message->LParam = value;
	}

	public void PreventRegionUpdate()
	{
		// To prevent Nc region update in DefWndProc, set (updateRegion)LParam = -1;
		UpdateRegion = new IntPtr(-1);
	}

	// When wParam == TRUE, result is ignored
	public bool CanPreventDefault => !IsActive;

	public unsafe NcActivateResult Result
	{
		get => (NcActivateResult)Message->Result.ToSafeInt32();
		set => Message->Result = new IntPtr((int)value);
	}
}

public struct NcCalcSizePacket : IPacket
{
	public unsafe WM MsgId => Message->Id;
	public unsafe void MarkAsHandled() => Message->MarkAsHandled();

	public unsafe WindowMessage* Message;

	public unsafe NcCalcSizePacket(WindowMessage* message)
	{
		Message = message;
	}

	public unsafe IntPtr Hwnd
	{
		get => Message->Hwnd;
		set => Message->Hwnd = value;
	}

	public unsafe bool ShouldCalcValidRects
	{
		get => Message->WParam.ToSafeInt32() > 0;
		set => Message->WParam = (value ? 1 : 0);
	}

	public unsafe ref NcCalcSizeParams Params => ref ((NcCalcSizeParamsWrapper*)Message->LParam)->Value;

	public unsafe Rectangle ParamsRect => *(Rectangle*)Message->LParam;

	public unsafe WindowViewRegionFlags Result
	{
		get => (WindowViewRegionFlags)Message->Result.ToSafeInt32();
		set => Message->Result = new IntPtr((int)value);
	}
}

#region Support Types

public enum MouseButton
{
	Left = 0x1,
	Right = 0x2,
	Middle = 0x4,
	Other = 0x8,
	XButton1 = 0x10 | Other,
	XButton2 = 0x20 | Other
}

public enum MouseButtonResult
{
	Default = 0,

	// Required to be set when application that processes XButton in order help simulation softwares
	// determine if the result was processed by application or DefWndProc
	Handled = 1
}

public enum EraseBackgroundResult
{
	// 1 - prevent default erase.
	// 0 - Let DefWndProc erase the background with the window class's brush.
	Default = 0,
	DisableDefaultErase = 1
}

public enum CreateWindowResult
{
	Default = 0,
	PreventCreation = -1
}

public enum AppCommandResult
{
	Default = 0,
	Handled = 1
}

public enum NcActivateResult
{
	// var result = TRUE // Default processing;
	// var result = FALSE // Prevent changes.
	Default = 1,
	PreventDefault = 0
}

#endregion

#region Ref-Wrapper

[StructLayout(LayoutKind.Sequential)]
internal struct WindowMessageWrapper
{
	public WindowMessage Value;
}

[StructLayout(LayoutKind.Sequential)]
internal struct WindowPositionWrapper
{
	public WindowPosition Value;
}

[StructLayout(LayoutKind.Sequential)]
internal struct CreateStructWrapper
{
	public CreateStruct Value;
}

[StructLayout(LayoutKind.Sequential)]
internal struct MinMaxInfoWrapper
{
	public MinMaxInfo Value;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NcCalcSizeParamsWrapper
{
	public NcCalcSizeParams Value;
}

#endregion



file static class PacketTypesExt
{
	public static int ToInt32(this Size size)
	{
		return (size.Height << 16) | size.Width;
	}

	public static int ToInt32(this Point point)
	{
		return (point.Y << 16) | point.X;
	}
}