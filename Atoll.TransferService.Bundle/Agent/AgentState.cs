using System.Net.Sockets;
using Atoll.TransferService.Bundle.Proto;

namespace Atoll.TransferService.Bundle.Agent
{
    public class AgentState: State
    {
        public Packet Packet;

        public Socket Socket;

        public AgentState(int bufferSize, string route, object contract) : base(bufferSize)
        {
            Packet = Packet.FromStruct(route, contract);
        }

        public void Send()
        {
            Packet.ToByteArray(ref Buffer);
            BufferLen = Packet.MySize;
        }
    }
}
