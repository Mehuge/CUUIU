using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace UIU
{
    public class Bot
    {
        public Bot()
        {
            var windows = Win32.FindWindowsWithText("Build: Beta ");

            if (windows.Count() > 0) {
                var hWnd = windows.First();
                Win32.SetForegroundWindow(hWnd);
                Thread.Sleep(500);
                Win32.SetFocus(hWnd);
                Thread.Sleep(500);
            }
        }
    }
}