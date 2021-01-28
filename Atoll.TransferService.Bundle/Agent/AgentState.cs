using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Atoll.TransferService.Bundle.Proto;
using Corallite.Glob;
using Newtonsoft.Json;
using TestContract;

namespace Atoll.TransferService.Bundle.Agent
{
    public class AgentState: State
    {
        public Socket Socket;

        public AgentState(int bufferSize, string route, Commands cmdId, object contract, IFs fs) : base(bufferSize)
        {
            this.fs =fs;
            Packet = Packet.FromStruct(route, contract, cmdId);
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
            if (RecvStream == null && Packet.StatusCode == HttpStatusCode.OK)
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

        public override T Result<T>()
        {
            RecvStream.Position = 0;
            using (StreamReader sr = new StreamReader(RecvStream))
            {
                var x = sr.ReadToEnd();
                return JsonConvert.DeserializeObject<T>(x, new JsonSerializerSettings(){Formatting = Formatting.Indented} );
            }
        }

        private IFs fs;

        public override void Close()
        {
            RecvStream?.Close();
            RecvStream = null;
            base.Close();
        }
    }
}
