﻿using System;

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
