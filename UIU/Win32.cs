using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

public class Win32
{
    const int PROCESS_WM_READ = 0x0010;

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

    [DllImport("user32", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private extern static bool EnumThreadWindows(int threadId, EnumWindowsProc callback, IntPtr lParam);

    [DllImport("user32", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
 
    [DllImport("kernel32.dll")]
    public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);

    public static byte PeekByte(Process process, IntPtr addr)
    {
        IntPtr processHandle = OpenProcess(PROCESS_WM_READ, false, process.Id);
        int bytesRead = 0;
        byte[] buffer = new byte[1]; 
        ReadProcessMemory((int)processHandle, (int)addr, buffer, buffer.Length, ref bytesRead);
        return buffer[0];
    }

    public static int PeekIntBE(Process process, IntPtr addr)
    {
        IntPtr processHandle = OpenProcess(PROCESS_WM_READ, false, process.Id);
        int bytesRead = 0;
        byte[] buffer = new byte[sizeof(int)];
        ReadProcessMemory((int)processHandle, (int)addr, buffer, sizeof(int), ref bytesRead);
        return buffer[0] << 24 & buffer[1] << 16 & buffer[2] << 8 & buffer[3];
    }

    public static int PeekIntLE(Process process, IntPtr addr)
    {
        IntPtr processHandle = OpenProcess(PROCESS_WM_READ, false, process.Id);
        int bytesRead = 0;
        byte[] buffer = new byte[sizeof(int)];
        ReadProcessMemory((int)processHandle, (int)addr, buffer, sizeof(int), ref bytesRead);
        return buffer[3] << 24 & buffer[2] << 16 & buffer[1] << 8 & buffer[0];
    }

    public static string PeekString(Process process, int addr)
    {
        string str = "";
        IntPtr processHandle = OpenProcess(PROCESS_WM_READ, false, process.Id);
        int bytesRead = 0;
        byte[] buffer = new byte[1];
        do {
            ReadProcessMemory((int)processHandle, addr, buffer, 1, ref bytesRead);
            str += (char)buffer[0];
            addr++;
        } while (buffer[0] != 0);
        return str;
    }

    public static string PeekUnicodeString(Process process, int addr)
    {
        string str = "";
        IntPtr processHandle = OpenProcess(PROCESS_WM_READ, false, process.Id);
        int bytesRead = 0;
        byte[] buffer = new byte[2048];
        ReadProcessMemory((int)processHandle, addr, buffer, buffer.Length, ref bytesRead);
        str = Encoding.Unicode.GetString(buffer);
        return str;
    }

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

    public static IEnumerable<Process> FindWindowsWithTextInTitle(string text)
    {
        IntPtr hWnd = IntPtr.Zero;
        List<Process> processes = new List<Process>();
        foreach (Process pList in Process.GetProcesses())
        {
            if (pList.MainWindowHandle != IntPtr.Zero)
            {
                if (pList.MainWindowTitle.Contains(text))
                {
                    processes.Add(pList);
                }
            }
        }
        return processes;
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
        System.TimeSpan delay = TimeSpan.FromMilliseconds((double)upDelay);
        SendKey(keyCode, KeyFlag.KeyDown);
        Thread.Sleep(delay);
        SendKey(keyCode, KeyFlag.KeyUp);
    }

    public struct Rect
    {
        public int Left { get; set; }
        public int Top { get; set; }
        public int Right { get; set; }
        public int Bottom { get; set; }
    }

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

    [DllImport("user32.dll")]
    public static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

    const int WM_MOUSEMOVE = 0x0200;
    const int WM_LBUTTONDOWN = 0x0201;
    const int WM_LBUTTONUP = 0x0202;
    const int WM_RBUTTONDOWN = 0x0204;
    const int WM_RBUTTONUP = 0x0205;

    const int
        MOUSEEVENTF_ABSOLUTE = 0x8000,
        MOUSEEVENTF_HWHEEL = 0x01000,
        MOUSEEVENTF_MOVE = 0x0001,
        MOUSEEVENTF_MOVE_NOCOALESCE = 0x2000,
        MOUSEEVENTF_LEFTDOWN = 0x0002,
        MOUSEEVENTF_LEFTUP = 0x0004,
        MOUSEEVENTF_RIGHTDOWN = 0x0008,
        MOUSEEVENTF_RIGHTUP = 0x0010,
        MOUSEEVENTF_MIDDLEDOWN = 0x0020,
        MOUSEEVENTF_MIDDLEUP = 0x0040,
        MOUSEEVENTF_VIRTUALDESK = 0x4000,
        MOUSEEVENTF_WHEEL = 0x0800,
        MOUSEEVENTF_XDOWN = 0x0080,
        MOUSEEVENTF_XUP = 0x0100;

    [DllImport("user32.dll")]
    static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

    [DllImport("user32.dll")]
    static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData,
       UIntPtr dwExtraInfo);

    public static void RightMove(IntPtr hWnd, int amount, int step, int delay)
    {
        Point p = Cursor.Position;
        mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, UIntPtr.Zero); /// right mouse button down
        int dir = 1;
        if (amount < 0) {
            dir = -1;
            amount = -amount;
        }
        for (int i = 0; i < amount; i += step) {
            mouse_event(MOUSEEVENTF_MOVE | MOUSEEVENTF_RIGHTDOWN, (uint) (dir*step), 0, 0, UIntPtr.Zero); /// right mouse button move
            Thread.Sleep(delay);
        }
        mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, UIntPtr.Zero); /// right mouse button up
        SetCursorPos(p.X, p.Y);
    }

    public static void LeftMouseClick(IntPtr hWnd, int delay)
    {
        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero); /// right mouse button down
        Thread.Sleep(delay);
        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero); /// right mouse button down
    }

}