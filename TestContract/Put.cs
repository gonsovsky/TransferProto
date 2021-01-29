using System;

namespace TestContract
{
    [Serializable]
    public struct PutContract
    {
        public string Url;

        public long Offset;

        public long Length;
    }
}
