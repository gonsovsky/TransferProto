using System;
using System.Net.Sockets;

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
