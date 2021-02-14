using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace TestClient
{
    internal static class ReUse
    {
        internal static Config cfg;


        internal static Socket Socket = null;
        internal static int LastTick = 0;

        public static Socket Put(Socket s)
        {
            Socket = s;
            LastTick = Environment.TickCount;
            return Socket;
        }

        public static Socket Get()
        {
            var p = Environment.TickCount <= LastTick + cfg.KeepAliveMsec ? Socket : null;
            return p;
        }
    }
}
