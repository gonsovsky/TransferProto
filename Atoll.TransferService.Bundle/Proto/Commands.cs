using System;

namespace Atoll.TransferService.Bundle.Proto
{
    [Flags]
    public enum Commands: byte
    {
        Custom = 0x0,
        Get = 0x1,
        List = 0x2,
        Put = 0x3
    }
}
