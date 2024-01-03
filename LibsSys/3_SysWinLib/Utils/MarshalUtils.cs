using System.Runtime.InteropServices;

namespace SysWinLib.Utils;

static class MarshalUtils
{
	public static T GetDelegate<T>(T fun) where T : Delegate => Marshal.GetDelegateForFunctionPointer<T>(Marshal.GetFunctionPointerForDelegate(fun));
}