using System;
using System.Collections.Generic;
using System.Text;

namespace TestClient
{
    public class Config
    {
        public int Port { get; set; }
        public string Net { get; set; }
        public int BufferSize { get; set; }
        public int KeepAlive { get; set; }

        public int KeepAliveMsec => KeepAlive * 1000;

        public bool IsKeepAlive => KeepAlive > 0;
    }
}
