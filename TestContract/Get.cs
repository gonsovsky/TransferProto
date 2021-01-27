using System;

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
