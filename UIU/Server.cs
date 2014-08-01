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
            switch (request[0])
            {
                case "echo":
                    SendMessage(socket, "echo " + message);
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
