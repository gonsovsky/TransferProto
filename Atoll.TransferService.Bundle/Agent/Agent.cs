using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Atoll.TransferService.Bundle.Proto;

namespace Atoll.TransferService.Bundle.Agent
{
    public class Agent: Party
    {
        private readonly string net;

        private readonly int port;

        private readonly int bufferSize;

        private readonly IFs fs;

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

        public object Cmd<T>(string route, T contract, Commands commandId=Commands.Custom, Stream data=null) where T: struct 
        {
            var state = new AgentState(bufferSize, route, commandId, contract, data, fs);
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
                SendDone.WaitOne();
                Receive(state);
                AllDone.WaitOne();
                if (state.Packet.StatusCode == HttpStatusCode.OK)
                    return Complete(state);
                else
                {
                    throw new ApplicationException($"{state.Packet.StatusCode}");
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

        private void Send(AgentState state)
        {
            state.Send();
            state.Socket.BeginSend(state.Buffer, 0, state.BufferLen, 0,
                SendCallback, state);
        }

        private void SendCallback(IAsyncResult ar)
        {
            var state = (AgentState)ar.AsyncState;
            try
            {
                state.Socket.EndSend(ar);
                SendDone.Set();
                OnRequest?.Invoke(this, state);
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
                    state.DataTransmitted(bytesRead);
                    client.BeginReceive(state.Buffer, 0, state.BufferSize, 0,
                        ReceiveCallback, state);
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

        public override void Abort(State state, Exception e)
        {
            state?.Close();
            state?.Dispose();
            ConnectDone.Reset();
            SendDone.Reset();
            base.Abort(state, e);
        }

        public override object Complete(State state)
        {
            OnResponse?.Invoke(this, state);
            state?.Close();
            state?.Dispose();
            ConnectDone.Reset();
            SendDone.Reset();
            AllDone.Reset();
            return null;
        }
    }
}