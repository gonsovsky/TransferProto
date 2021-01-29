using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Atoll.TransferService.Bundle.Server.Contract;
using Atoll.TransferService.Bundle.Server.Handler;

namespace TestServer.Handlers
{
    public abstract class Custom : IHandler
    {
        protected Stream Stream;

        protected bool Ready
        {
            get
            {
                if (Stream == null)
                    return true;
                return Stream.Position == Stream.Length;
            }
        }

        public virtual void Dispose()
        {
            try { this.Stream?.Close(); } catch { /* IGNORED */ }
            try { this.Stream?.Dispose(); } catch { /* IGNORED */ }
            this.Stream = null;
        }

        public virtual IContext Open(IContext ctx)
        {
            throw new NotImplementedException();
        }

        public virtual void Close(IContext ctx)
        {
            Stream?.Close();
            Stream = null;
        }

        public abstract IContext Read(IContext ctx);

        public abstract IContext Write(IContext ctx);
    
        public virtual bool DataSent(IContext ctx)
        {
            throw new NotImplementedException();
        }

        public virtual bool DataReceived(IContext ctx)
        {
            throw new NotImplementedException();
        }
    }
}
