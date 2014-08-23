using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace UIU
{
    public class Bot
    {
        Process client = null;

        public Bot()
        {
            var processes = Win32.FindWindowsWithTextInTitle("Build: Internal ");
            if (processes.Count() > 0)
            {
                client = processes.First();
                Win32.SetForegroundWindow(client.MainWindowHandle);
                Thread.Sleep(10);
                Win32.SetFocus(client.MainWindowHandle);
                Thread.Sleep(10);
            }
        }

        internal void SendKey(int keyCode, int delay)
        {
            if (null != client)
            {
                Win32.SetForegroundWindow(client.MainWindowHandle);
                Thread.Sleep(50);
                Win32.SetFocus(client.MainWindowHandle);
                Thread.Sleep(10);
                Console.WriteLine("KeyCode: " + keyCode + " delay " + delay);
                Win32.PressKey((short)keyCode, delay);
            }
        }

        internal string ReadMemory(int addr)
        {
            string content = Win32.PeekUnicodeString(client, addr);
            return content;
        }

        internal void Turn(int amount, int step, int delay)
        {
            if (null != client)
            {
                Win32.SetForegroundWindow(client.MainWindowHandle);
                Thread.Sleep(50);
                Win32.SetFocus(client.MainWindowHandle);
                Thread.Sleep(10);
                Console.WriteLine("Turn " + amount + " step " + step + " delay " + delay);
                Win32.RightMove(client.MainWindowHandle, amount, step, delay);
            }
        }

        internal void Click(int delay)
        {
            if (null != client)
            {
                Win32.SetForegroundWindow(client.MainWindowHandle);
                Thread.Sleep(50);
                Win32.SetFocus(client.MainWindowHandle);
                Thread.Sleep(10);
                Console.WriteLine("Click " + delay);
                Win32.LeftMouseClick(client.MainWindowHandle, delay);
            }
        }

    }
}