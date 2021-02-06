using System;

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
                    //if (state.Packet.CommandId == (byte)Commands.List)
                    //    Console.WriteLine($"\r\n{state.Result<FsInfo[]>().ToJson()}");
                    //else
                        Console.WriteLine(
                            $@"Agent Response {state.SendPacket.Route}:  {state.SendPacket.Url()} - {state.StatusCode}");
                },
                OnAbort = (party, state, ex) =>
                {
                    Console.WriteLine($"Agent Abort {state.SendPacket.Route}: {ex.Message}");
                }
            };

            //var up = new PutContract()
            //{
            //    Url = "456.txt",
            //    Offset = 0,
            //    Length = new FileInfo("456.txt").Length
            //};
            //agent.Cmd("put", up, Commands.Put,
            //    new FileStream(
            //        "456.txt", FileMode.Open, FileAccess.Read, FileShare.Read)
            //);

            //Console.WriteLine("Put ready");


            var result = agent.Cmd("download", "123.txt", 0,500000);

            //t.Url = "abc.txt";
            //agent.Cmd("get", t, Commands.Get);
            //t.Url = "../../../../../../pagefile.sys";
            //agent.Cmd("get", t, Commands.Get);

            //agent.Cmd("list", new ListContract() {Url = "/"}, Commands.List);

            Console.WriteLine("enter");
            Console.ReadLine();
        }
    }
}
