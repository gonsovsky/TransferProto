using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TestServer
{
    public abstract class MyHotFileHandler
    {
        protected MyHotFileHandler()
        {
            HomeDir = AppDomain.CurrentDomain.BaseDirectory;
#if NETSTANDARD
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == false)
                HomeDir = "/";
#endif
        }

        protected string HomeDir;

        public static string AbsoluteToRelativePath(string pathToFile, string referencePath)
        {
            var fileUri = new Uri(pathToFile);
            var referenceUri = new Uri(referencePath);
            return referenceUri.MakeRelativeUri(fileUri).ToString();
        }

        protected string AbsPath(string path)
        {
            path = path.TrimStart('\\').TrimStart('/');
            return Path.Combine(HomeDir, path);
        }
    }
}
