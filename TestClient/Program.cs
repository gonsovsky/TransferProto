using System;

namespace TestClient
{
    internal class Program
    {
        private static void Main()
        {
            var fs = new Fs(Helper.AssemblyDirectory);
            var agent = new Agent(3000, "localhost", 256, fs)
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


            agent.Cmd("download", "abc.txt", 0, 500000);

            agent.Cmd("download", "../../../../../../pagefile.sys", 0, 500000);

            agent.Cmd("download", "123.txt", 0, 500000);

            agent.Cmd("list", "/", 0,0);

            Console.WriteLine("enter to close");
            Console.ReadLine();
        }
    }
}
