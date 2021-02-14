using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;

namespace TestClient
{
    public delegate void StateEvent(Agent sender, AgentState state);

    public delegate void StateErrorEvent(Agent sender, AgentState state, Exception ex);

    public class Agent
    {
        private readonly Config cfg;

        private readonly IFs fs;

        public StateEvent OnRequest { get; set; }

        public StateEvent OnResponse { get; set; }

        public StateErrorEvent OnAbort { get; set; }

        public Agent(Config cfg, IFs fs)
        {
            this.cfg = cfg;
            this.fs = fs;
            ReUse.cfg = cfg;
        }

        protected readonly ManualResetEvent ConnectDone =
            new ManualResetEvent(false);

        protected readonly ManualResetEvent SendDone =
            new ManualResetEvent(false);

        protected readonly ManualResetEvent ReceiveDone =
            new ManualResetEvent(false);

        public object Cmd<T>(string route, T contract, Stream data=null) where T: Contract
        {
            ConnectDone.Reset();
            SendDone.Reset();
            ReceiveDone.Reset();
            var state = new AgentState(cfg, route, contract, data, fs);
            try
            {
                Socket client;
                if (!cfg.IsKeepAlive)
                    client = NewSocket(state);
                else
                {
                    client = ReUse.Get();
                    if (client == null)
                    {
                        client = NewSocket(state);
                        ReUse.Put(client);
                    }
                }
                state.Socket = client;
                Send(state);
                SendDone.WaitOne();
                Receive(state);
                ReceiveDone.WaitOne();
                if (state.RecvPacket.StatusCode == HttpStatusCode.OK)
                    return Complete(state);
                else
                {
                    throw new ApplicationException($"{state.RecvPacket.StatusCode}, {state.RecvPacket.Body}");
                }
            }
            catch (Exception e)
            {
                Abort(state, e);
                return null;
            }
        }

        protected Socket NewSocket(AgentState state)
        {
            var ipHostInfo = Dns.GetHostEntry(cfg.Net);
            var ipAddress = ipHostInfo.AddressList
                .First(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            var remoteEp = new IPEndPoint(ipAddress, cfg.Port);
            var client = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);
            state.Socket = client;
            client.BeginConnect(remoteEp, ConnectCallback, state);
            //if (cfg.IsKeepAlive)
            //    client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            ConnectDone.WaitOne();
            return client;
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            var handler = (AgentState)ar.AsyncState;
            try
            {
                handler.Socket.EndConnect(ar);
                ConnectDone.Set();
            }
            catch (Exception e)
            {
                Abort(handler, e);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Send(AgentState state)
        {
            if (state.Send())
                state.Socket.BeginSend(state.Buffer, 0, state.BufferLen, 0,
                SendCallback, state);
        }

        private void SendCallback(IAsyncResult ar)
        {
            var state = (AgentState)ar.AsyncState;
            state.Socket.EndSend(ar);
            try
            {
                if (state.HasSend() == false)
                {
                    SendDone.Set();
                    OnRequest?.Invoke(this, state);
                }
                else
                {
                    Send(state);
                }
            }
            catch (Exception e)
            {
                Abort(state, e);
            }
        }

        private void Receive(AgentState state)
        {
            try
            {
                state.Socket.BeginReceive(state.Buffer, 0, state.BufferSize, 0,
                    ReceiveCallback, state);
            }
            catch (Exception e)
            {
                Abort(state, e);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            var state = (AgentState)ar.AsyncState;
            try
            {
                var client = state.Socket;
                var bytesRead = client.EndReceive(ar);
                if (bytesRead > 0)
                {
                    state.DataReceived(bytesRead);
                    if (state.RecvPacket != null && state.RecvPacket.StatusCode != HttpStatusCode.OK)
                    {
                        ReceiveDone.Set();
                        return;
                    }

                    if (state.SendPacket.Route != "upload")
                    {
                        if (state.RecvPacket != null && state.RecvPacket.StatusCode == HttpStatusCode.OK
                                                     && state.RecvStream.Length == state.RecvPacket.DataSize)
                        {
                            ReceiveDone.Set();
                            return;
                        }
                        else
                        {
                            client.BeginReceive(state.Buffer, 0, state.BufferSize, 0,
                                ReceiveCallback, state);
                        }
                    }
                    else
                    {
                        ReceiveDone.Set();
                    }
                }
                else
                {
                    ReceiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Abort(state, e);
            }
        }

        public void Abort(AgentState state, Exception e)
        {
            state?.Dispose();
            ConnectDone.Set();
            SendDone.Set();
            ReceiveDone.Set();
            OnAbort?.Invoke(this, state, e);
        }

        public object Complete(AgentState state)
        {
            OnResponse?.Invoke(this, state);
            state?.Dispose();
            ConnectDone.Set();
            SendDone.Set();
            ReceiveDone.Set();
            return null;
        }
    }
}