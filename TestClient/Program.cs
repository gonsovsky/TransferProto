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
                    //Console.WriteLine($"Agent Request : {state.Url} [{state.Gram.Start}/{state.Gram.Length}]");
                },
                OnResponse = (party, state) =>
                {
                    //Console.WriteLine($"Agent Response: {state.Url} [{state.Gram.Start}/{state.Gram.Length}]");
                },
                OnAbort = (party, state, ex) =>
                {
                    //Console.WriteLine(
                    //    $"Agent Abort   : {state.Url} [{state.Gram.Start}/{state.Gram.Length}] {ex.Message}");
                }
            };
            var t = new GetContract()
            {
                Url = "123.txt",
                Offset = 0,
                Length = 500000
            };
            var result = agent.Cmd("download", t);
            Console.ReadLine();
        }
    }
}
