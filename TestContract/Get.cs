﻿using System;
using System.Runtime.InteropServices;

namespace TestContract
{
    [Serializable]
    public struct GetContract
    {
        public string Url;

        public long Offset;

        public long Length;
    }
}
