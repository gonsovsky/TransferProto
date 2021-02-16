using System;
using System.IO;

namespace TestClient
{
    internal class Program
    {
        private static void Main()
        {
            var fs = new Fs(Helper.AssemblyDirectory);
            var agent = new Agent(new Config()
                { BufferSize = 512, KeepAlive = 5, Net = "localhost", Port = 3000}, 
                fs)
            {
                OnRequest = (party, state) =>
                {
                    Console.WriteLine(
                        $"Agent Request {state.SendPacket.Route}: {state.SendPacket.Url()}");
                },
                OnResponse = (party, state) =>
                {
                    Console.WriteLine(state.SendPacket.Route == "list"
                        ? $"\r\n{state.StringResult}"
                        : $@"Agent Response {state.SendPacket.Route}:  {state.SendPacket.Url()} - {state.StatusCode}");
                },
                OnAbort = (party, state, ex) =>
                {
                    Console.WriteLine($"Agent Abort {state.SendPacket.Route}: {ex.Message}");
                }
            };

            

            while (true)
            {
                agent.Cmd("download", new Contract() {Url = "abc.txt"});

                agent.Cmd("download", new Contract() {Url = "../../../../../../pagefile.sys"});

                agent.Cmd("download", new Contract() {Url = "123.txt", Offset = 0, Length = 1500, File = "123-1.txt"});

                agent.Cmd("download", new Contract() {Url = "123.txt", Offset = 1500, Length = 0, File = "123-2.txt"});

                Helper.Combine(Helper.AssemblyDirectory,
                    new[] {"123-1.txt", "123-2.txt"}, "123.txt");

                agent.Cmd("list", new Contract() {Url = "/"});

                agent.Cmd("list", new Contract() {Url = "/subfolder"});

                agent.Cmd("download", new Contract() {Url = "/subfolder/subfile.txt"});

                agent.Cmd("delete", new Contract() {Url = "abrakadabra"});

                agent.Cmd("upload",
                    new Contract()
                    {
                        Url = "456.txt",
                        Offset = 0,
                        Length = new FileInfo("456.txt").Length
                    }
                    ,
                    new FileStream("456.txt", FileMode.Open, FileAccess.Read, FileShare.Read)
                );

            }

            Console.WriteLine("enter to close");
            Console.ReadLine();
        }
    }
}
