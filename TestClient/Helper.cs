using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace TestClient
{
    public static class Helper
    {
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public static string Url(this SendPacket packet)
        {
            return Encoding.UTF8.GetString(packet.BodyData, 0, packet.BodyLen);
        }
    }
}
