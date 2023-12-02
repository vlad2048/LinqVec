namespace LinqVec.Utils.WinForms_;

static class KeyUtils
{
    public static bool IsShiftPressed => (Control.ModifierKeys & Keys.Shift) != 0;
    public static bool IsCtrlPressed => (Control.ModifierKeys & Keys.Control) != 0;
    public static bool IsAltPressed => (Control.ModifierKeys & Keys.Alt) != 0;
}