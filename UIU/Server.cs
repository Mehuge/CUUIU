using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Fleck;
using UIU;

namespace UIU
{
    class Server
    {
        List<IWebSocketConnection> allSockets = new List<IWebSocketConnection>();
        WebSocketServer server = null;
        Bot bot = null;
        public Server()
        {
            server = new WebSocketServer("ws://0.0.0.0:8888");
            bot = new Bot();
        }
        void ProcessCommand(IWebSocketConnection socket, string message)
        {
            List<string> request = JSON.parse(message);
            int delay;
            switch (request[0])
            {
                case "echo":
                    SendMessage(socket, "echo " + message);
                    break;
                case "keypress":
                    delay = request.Count > 2 && request[2] != null ? Int32.Parse(request[2]) : 100;
                    if (request[1] != null)
                    {
                        bot.SendKey(Int32.Parse(request[1]), delay);
                        SendMessage(socket, "OK");
                    }
                    else
                    {
                        SendMessage(socket, "INVALID KEYCODE");
                    }
                    break;
                case "turn":
                    if (request.Count > 1 && request[1] != null)
                    {
                        int amount = Int32.Parse(request[1]);
                        int step = request.Count > 2 && request[2] != null ? Int32.Parse(request[2]) : 10;
                        delay = request.Count > 3 && request[3] != null ? Int32.Parse(request[3]) : 10; 
                        bot.Turn(amount, step, delay);
                        SendMessage(socket, "OK");
                    }
                    else { SendMessage(socket, "INVALID TURN AMOUNT"); }
                    break;
                case "click":
                    if (request.Count > 1 && request[1] != null)
                    {
                        delay = Int32.Parse(request[1]);
                        bot.Click(delay);
                        SendMessage(socket, "OK");
                    }
                    else { SendMessage(socket, "INVALID DELAY"); }
                    break;
                case "memstr":
                    int addr = request.Count > 1 && request[1] != null ? Int32.Parse(request[1], System.Globalization.NumberStyles.AllowHexSpecifier) : 0;
                    Console.WriteLine(addr.ToString("X8"));
                    string mem = bot.ReadMemory(addr);
                    SendMessage(socket, mem);
                    break;
            }
        }
        void SendMessage(IWebSocketConnection socket, string message)
        {
            Console.WriteLine(message);
            socket.Send(message);
        }
        void BroadcastMessage(IWebSocketConnection socket, string message)
        {
            Console.WriteLine(message);
            allSockets.ToList().ForEach(s => SendMessage(s,message));
        }
        public void run()
        {
            FleckLog.Level = LogLevel.Debug;
            server.Start(socket => {
                socket.OnOpen = () => {
                    Console.WriteLine("Open!");
                    allSockets.Add(socket);
                };
                socket.OnClose = () => {
                    Console.WriteLine("Close!");
                    allSockets.Remove(socket);
                };
                socket.OnMessage = message => {
                    if (message.StartsWith("C ")) {
                        ProcessCommand(socket, message.Substring(2));
                    } else if (message.StartsWith("B ")) {
                        BroadcastMessage(socket, message.Substring(2));
                    } else if (message.StartsWith("M ")) {
                        SendMessage(socket, message.Substring(2));
                    } else { 
                        // process other message types here
                    }
                };
            });

            var input = Console.ReadLine();
            while (input != "exit")
            {
                foreach (var socket in allSockets.ToList())
                {
                    SendMessage(socket, input);
                }
                input = Console.ReadLine();
            }
        }
    }
}
