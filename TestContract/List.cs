using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace TestContract
{
    [Serializable]
    public struct ListContract
    {
        public string Url;

        public long Offset;

        public long Length;
    }
}
