using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Atoll.TransferService.Bundle.Proto;
using Newtonsoft.Json;

namespace Atoll.TransferService.Bundle.Agent
{
    public class AgentState: State
    {
        public Socket Socket;

        public AgentState(int bufferSize, string route, Commands cmdId, object contract, Stream data, IFs fs) : base(bufferSize)
        {
            this.fs =fs;
            this.SendData = data;
            Packet = Packet.FromStruct(route, cmdId, contract, data );
        }

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
                    case Commands.Put:
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

        public override bool Send()
        {
            if (base.Send())
                return true;
            if (!HasSend())
                return false;
            //SendData.Seek(bytesSent, SeekOrigin.Begin);
            var len = (int)Math.Min(BufferSize, (SendData.Length - SendData.Position));
            var cnt = SendData.Read(Buffer, 0, len);
            bytesSent += 1;
            BufferLen = len;
            return true;
        }

        private int bytesSent; 

        public override bool HasSend()
        {
            if (HeadSent == false)
                return true;
            if (SendData == null)
                return false;
            if (Packet.CommandId != Commands.Put || SendData == null)
                return false;
            return SendData.Position < SendData.Length;
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

        private readonly IFs fs;

        protected Stream SendData; 

        public override void Close()
        {
            RecvStream?.Close();
            RecvStream = null;
            base.Close();
        }
    }
}
