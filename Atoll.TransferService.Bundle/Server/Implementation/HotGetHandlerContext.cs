﻿using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Corallite.Buffers;

namespace Atoll.TransferService
{
    public class HotGetHandlerContext: HotContext, IHotGetHandlerContext, IDisposable
    {
        public IHotGetHandler Handler;

        public HotGetHandlerContext(HotContext source): base(source.Socket, source.Config)
        {
            this.Accept = source.Accept;
            this.Buffer = source.Buffer;
            this.BufferSize = source.BufferSize;
            Request = new HotGetHandlerRequest(Accept.Route, Accept.BodyData, Accept.BodyLen);
            Frame = new HotGetHandlerFrame(this);
            this.Frame.ContentLength = source.Accept.ContentLength;
            this.Frame.ContentOffset = source.Accept.ContentOffset;
        }

        public override void Dispose()
        {
            if (Buffer != null)
                UniArrayPool<byte>.Shared.Return(this.Buffer);
            Buffer = null;
            Socket?.Shutdown(SocketShutdown.Both);
            Socket?.Close();
            Socket = null;
            Handler?.Dispose();
            Handler = null;
            Request?.Dispose();
            Request = null;
            Frame?.Dispose();
            Frame = null;
        }

        public override bool DataReceived(int cnt)
        {
            return false;
        }

        public override bool DataSent()
        {
            if (base.DataSent())
                return true;
            if (SendBytes == 0)
                return false;
            Frame.Count = BufferSize;
            this.Handler.Read(this);
            return true;
        }

        public override byte[] SendData => Frame.Buffer;

        public override int SendBytes
        {
            get => Frame.BytesRead;
            set => Frame.BytesRead = value;
        }

        public HotGetHandlerRequest Request { get; set; }

        public HotGetHandlerFrame Frame { get; set; }

        public IHotGetHandlerContext Ok()
        {
            ResponseStatus = HttpStatusCode.OK;
            ResponseMessage = "";
            return this;
        }

        public IHotGetHandlerContext BadRequest(string message = "")
        {
            ResponseStatus = HttpStatusCode.BadRequest;
            ResponseMessage = message;
            return this;
        }

        public IHotGetHandlerContext NotFound(string message = "")
        {
            ResponseStatus = HttpStatusCode.NotFound;
            ResponseMessage = message;
            return this;
        }

        public IHotGetHandlerContext Error(string message = "")
        {
            ResponseStatus = HttpStatusCode.InternalServerError;
            ResponseMessage = message;
            return this;
        }

        public IHotGetHandlerContext NotImplemented(string message = "")
        {
            ResponseStatus = HttpStatusCode.NotImplemented;
            ResponseMessage = message;
            return this;
        }
    }
}
