using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace UIU
{
    public class Bot
    {
        IntPtr hWnd;

        public Bot()
        {
            var windows = Win32.FindWindowsWithTextInTitle("Build: Internal ");

            if (windows.Count() > 0) {
                hWnd = windows.First();
                Win32.SetForegroundWindow(hWnd);
                Thread.Sleep(10);
                Win32.SetFocus(hWnd);
                Thread.Sleep(10);
            }
        }

        internal void SendKey(int keyCode, int delay)
        {
            if (null != hWnd)
            {
                Win32.SetForegroundWindow(hWnd);
                Thread.Sleep(50);
                Win32.SetFocus(hWnd);
                Thread.Sleep(10);
                Console.WriteLine("KeyCode: " + keyCode + " delay " + delay);
                Win32.PressKey((short)keyCode, delay);
            }

        }
    }
}