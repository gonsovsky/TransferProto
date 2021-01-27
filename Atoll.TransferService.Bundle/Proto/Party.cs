using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Atoll.TransferService.Bundle.Proto
{
    public delegate void StateEvent(Party sender, State state);

    public delegate void StateErrorEvent(Party sender, State state, Exception ex);

    public class Party: IDisposable
    {
        public StateEvent OnRequest { get; set; }

        public StateEvent OnResponse { get; set; }

        public StateErrorEvent OnAbort { get; set; }

        protected readonly ManualResetEvent AllDone =
            new ManualResetEvent(false);

        public virtual void Abort(State state, Exception e)
        {
            //state?.Abort(e);
            AllDone.Reset();
            OnAbort?.Invoke(this, state, e);
        }

        public virtual object Complete(State state)
        {
            //var res = state.Complete();
            OnResponse?.Invoke(this, state);
            AllDone.Reset();
            return null;
        }

        public void Dispose()
        {
            AllDone?.Dispose();
        }
    }
}
