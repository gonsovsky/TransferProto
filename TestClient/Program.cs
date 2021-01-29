using System;
using Atoll.TransferService.Bundle.Agent;
using Atoll.TransferService.Bundle.Proto;
using TestContract;

namespace TestClient
{
    internal class Program
    {
        private static void Main()
        {
            var fs = new Fs(Helper.AssemblyDirectory);
            var agent = new Agent(3000, "localhost", 3000, fs)
            {
                OnRequest = (party, state) =>
                {
                    Console.WriteLine(
                        $"Agent Request {state.Packet.CommandId}: {state.Packet.ToStruct<GetContract>().Url}");
                },
                OnResponse = (party, state) =>
                {
                    if (state.Packet.CommandId == Commands.List)
                        Console.WriteLine($"\r\n{state.Result<FsInfo[]>().ToJson()}");
                    else
                        Console.WriteLine(
                            $@"Agent Response {state.Packet.CommandId}:  {state.Packet.ToStruct<GetContract>().Url} - {state.StatusCode}");
                },
                OnAbort = (party, state, ex) =>
                {
                    Console.WriteLine($"Agent Abort {state.Packet.CommandId}: {ex.Message}");
                }
            };
            /*
            var up = new PutContract()
            {
                Url = "456.txt",
                Offset = 0,
                Length = 500000
            };
            agent.Cmd("put", up, Commands.Put,
                new FileStream(
                    "456.txt", FileMode.Open, FileAccess.Read, FileShare.Read)
            );

            Console.ReadLine();
            */
            var t = new GetContract()
            {
                Url = "123.txt",
                Offset = 0,
                Length = 500000
            };
            var result = agent.Cmd("get", t, Commands.Get);
            t.Url = "abc.txt";
            agent.Cmd("get", t, Commands.Get);
            t.Url = "../../../../../../pagefile.sys";
            agent.Cmd("get", t, Commands.Get);

            agent.Cmd("list", new ListContract() {Url = "/"}, Commands.List);

            Console.ReadLine();
        }
    }
}
