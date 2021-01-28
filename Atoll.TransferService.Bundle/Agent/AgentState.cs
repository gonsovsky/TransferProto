using System;
using System.IO;
using System.Net.Sockets;
using Atoll.TransferService.Bundle.Proto;
using Corallite.Glob;
using TestContract;

namespace Atoll.TransferService.Bundle.Agent
{
    public class AgentState: State
    {
        public Packet Packet;

        public Socket Socket;

        public AgentState(int bufferSize, string route, Commands cmdId, object contract, IFs fs) : base(bufferSize)
        {
            this.fs =fs;
            Packet = Packet.FromStruct(route, contract, cmdId);
        }

        public void Send()
        {
            Packet.ToByteArray(ref Buffer);
            BufferLen = Packet.MySize;
        }

        protected bool HeadRecv;

        protected Stream RecvStream;

        public override bool DataTransmitted(int len)
        {
            base.DataTransmitted(len);
            var headDelta = 0;
            if (!HeadRecv)
            {
                HeadRecv = true;
                if (BufferLen < Packet.MinSize)
                    return false;
                Packet = Packet.FromByteArray(Buffer);
                headDelta = Packet.MySize;
            }
            if (RecvStream == null)
            {
                switch ((Commands)Packet.CommandId)
                {
                    case Commands.List:
                    case Commands.Head:
                        RecvStream = new MemoryStream();
                        break;
                    case Commands.Get:
                        RecvStream = fs.Put(FileName);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            RecvStream?.Write(Buffer, headDelta, len - headDelta);
            return true;
        }

        public string FileName
        {
            get
            {
                var contract = Packet.ToStruct<GetContract>();
                return Path.Combine(Helper.AssemblyDirectory, contract.Url);
            }
        }

        private IFs fs;
    }
}
