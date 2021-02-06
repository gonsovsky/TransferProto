using System;
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
        private readonly string net;

        private readonly int port;

        private readonly int bufferSize;

        private readonly IFs fs;

        public StateEvent OnRequest { get; set; }

        public StateEvent OnResponse { get; set; }

        public StateErrorEvent OnAbort { get; set; }

        public Agent(int port, string net, int bufferSize, IFs fs)
        {
            this.port = port;
            this.net = net;
            this.bufferSize = bufferSize;
            this.fs = fs;
        }

        protected readonly ManualResetEvent ConnectDone =
            new ManualResetEvent(false);

        protected readonly ManualResetEvent SendDone =
            new ManualResetEvent(false);

        protected readonly ManualResetEvent AllDone =
            new ManualResetEvent(false);

        public object Cmd(string route, string url, long offset, long length, Stream data=null)
        {
            ConnectDone.Reset();
            SendDone.Reset();
            AllDone.Reset();
            var state = new AgentState(bufferSize, route, url, offset, length, data, fs);
            try
            {
                var ipHostInfo = Dns.GetHostEntry(net);
                var ipAddress = ipHostInfo.AddressList
                    .First(ip => ip.AddressFamily == AddressFamily.InterNetwork);
                var remoteEp = new IPEndPoint(ipAddress, port);
                var client = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);
                state.Socket = client;
                client.BeginConnect(remoteEp, ConnectCallback, state);
                ConnectDone.WaitOne();
                Send(state);
                Receive(state);
                AllDone.WaitOne();
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
                    AllDone.Set();
                    //client.BeginReceive(state.Buffer, 0, state.BufferSize, 0,
                    //   ReceiveCallback, state);
                }
                else
                {
                    AllDone.Set();
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
            AllDone.Set();
            OnAbort?.Invoke(this, state, e);
        }

        public object Complete(AgentState state)
        {
            OnResponse?.Invoke(this, state);
            state?.Dispose();
            ConnectDone.Set();
            SendDone.Set();
            AllDone.Set();
            return null;
        }
    }
}