using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

public class Win32
{
    public class Modifiers
    {
        public const int KEYEVENTF_EXTENDEDKEY = 0x0001; // Key down flag
        public const int KEYEVENTF_KEYUP = 0x0002;
    }

    public class Keys
    {
        public const int VK_LCONTROL = 0xA2;
        public const int VK_TAB = 0x09;
        public const int A = 0x41;
        public const int C = 0x43;
    }

    public class DirectInputKeys
    {
        public const int ESCAPE = 0x01;
        public const int SPACEBAR = 0x39;
        public const int TAB = 0x0F;
    }

    [DllImport("user32.dll")]
    public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern IntPtr SetFocus(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int GetWindowTextLength(IntPtr hWnd);

    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    public static string GetWindowText(IntPtr hWnd)
    {
        int size = GetWindowTextLength(hWnd);
        if (size++ > 0)
        {
            var builder = new StringBuilder(size);
            GetWindowText(hWnd, builder, builder.Capacity);
            return builder.ToString();
        }

        return String.Empty;
    }

    public static IEnumerable<IntPtr> FindWindowsWithText(string text)
    {
        IntPtr found = IntPtr.Zero;
        List<IntPtr> windows = new List<IntPtr>();

        EnumWindows(delegate(IntPtr wnd, IntPtr param)
        {
            if (GetWindowText(wnd).Contains(text))
            {
                windows.Add(wnd);
            }
            return true;
        }, IntPtr.Zero);

        return windows;
    }

    [DllImport("user32.dll")]
    static extern UInt32 SendInput(UInt32 nInputs, [MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] Input[] pInputs, Int32 cbSize);

    [StructLayout(LayoutKind.Sequential)]
    struct MouseInput
    {
        public int dx;
        public int dy;
        public int mouseData;
        public int dwFlags;
        public int time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct KeyboardInput
    {
        public short wVk; // virtual key code (not needed for direct input)
        public short wScan; // directx keycode
        public int dwFlags; // keyup, keydown
        public int time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct HardwareInput
    {
        public int uMsg;
        public short wParamL;
        public short wParamH;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct Input
    {
        [FieldOffset(0)]
        public int type;
        [FieldOffset(4)]
        public MouseInput mi;
        [FieldOffset(4)]
        public KeyboardInput ki;
        [FieldOffset(4)]
        public HardwareInput hi;
    }

    [Flags]
    public enum KeyFlag
    {
        KeyDown = 0x0000,
        ExtendedKey = 0x0001,
        KeyUp = 0x0002,
        UniCode = 0x0004,
        ScanCode = 0x0008
    }

    public static void SendInput(short keyCode, KeyFlag keyFlag)
    {
        var inputData = new Input[1];

        inputData[0].type = 1;
        inputData[0].ki.wScan = keyCode;
        inputData[0].ki.dwFlags = (int)keyFlag;
        inputData[0].ki.time = 0;
        inputData[0].ki.dwExtraInfo = IntPtr.Zero;

        SendInput(1, inputData, Marshal.SizeOf(typeof(Input)));
    }

    public static void SendKey(short keyCode, KeyFlag keyFlag)
    {
        SendInput(keyCode, keyFlag | KeyFlag.ScanCode);
    }

    public static void SendKeyDown(short keyCode)
    {
        SendKey(keyCode, KeyFlag.KeyDown);
    }

    public static void SendKeyUp(short keyCode)
    {
        SendKey(keyCode, KeyFlag.KeyUp);
    }

    public static void PressKey(short keyCode, long upDelay)
    {
        SendKey(keyCode, KeyFlag.KeyDown);
        Thread.Sleep(100);
        SendKey(keyCode, KeyFlag.KeyUp);
    }
}