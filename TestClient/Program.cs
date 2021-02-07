using System;
using System.IO;

namespace TestClient
{
    internal class Program
    {
        private static void Main()
        {
            var fs = new Fs(Helper.AssemblyDirectory);
            var agent = new Agent(3000, "localhost", 1024, fs)
            {
                OnRequest = (party, state) =>
                {
                    Console.WriteLine(
                        $"Agent Request {state.SendPacket.Route}: {state.SendPacket.Url()}");
                },
                OnResponse = (party, state) =>
                {
                    if (state.SendPacket.Route == "list")
                        Console.WriteLine($"\r\n{state.StringResult}");
                    else
                        Console.WriteLine(
                            $@"Agent Response {state.SendPacket.Route}:  {state.SendPacket.Url()} - {state.StatusCode}");
                },
                OnAbort = (party, state, ex) =>
                {
                    Console.WriteLine($"Agent Abort {state.SendPacket.Route}: {ex.Message}");
                }
            };


            agent.Cmd("download", new Contract() { Url = "abc.txt" });

            agent.Cmd("download", new Contract() { Url = "../../../../../../pagefile.sys" });

            var a = new Contract() { Url = "123.txt", Offset = 0, Length = 1500, File = "123-1.txt"};
            agent.Cmd("download", a);

            a = new Contract() { Url = "123.txt", Offset = 1500, Length = 0, File = "123-1.txt" };
            agent.Cmd("download", a);

            Helper.Combine(Helper.AssemblyDirectory,
                new[] { "123-1.txt", "123-2.txt" }, "123.txt");

            agent.Cmd("list", new Contract() { Url = "/" });

            agent.Cmd("delete", new Contract(){Url = "abrakadabra"});

            a = new Contract()
            {
                Url = "456.txt",
                Offset = 0,
                Length = new FileInfo("456.txt").Length
            };
            agent.Cmd("upload", a,
                new FileStream("456.txt", FileMode.Open, FileAccess.Read, FileShare.Read)
            );

            Console.WriteLine("enter to close");
            Console.ReadLine();
        }
    }
}
