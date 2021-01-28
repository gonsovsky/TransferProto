using System;
using System.Threading;

namespace Atoll.TransferService.Bundle.Proto
{
    public delegate void StateEvent(Party sender, State state);

    public delegate void StateErrorEvent(Party sender, State state, Exception ex);

    public class Party
    {
        public StateEvent OnRequest { get; set; }

        public StateEvent OnResponse { get; set; }

        public StateErrorEvent OnAbort { get; set; }

        protected readonly ManualResetEvent AllDone =
            new ManualResetEvent(false);

        public virtual void Abort(State state, Exception e)
        {
            OnAbort?.Invoke(this, state, e);
            state?.Close();
            state?.Dispose();
            AllDone.Reset();
        }

        public virtual object Complete(State state)
        {
            OnResponse?.Invoke(this, state);
            AllDone.Reset();
            return null;
        }
    }
}
