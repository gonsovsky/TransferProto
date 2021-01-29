using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;

namespace Atoll.TransferService.Bundle.Proto
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Packet
    {
        public Commands CommandId;

        public HttpStatusCode StatusCode;

        public short RouteLen;

        public byte[] RouteData;

        public short HeadLen;

        public byte[] HeadData;

        public long DataLen;

        public string Route
        {
            get => Encoding.ASCII.GetString(RouteData);
            set
            {
                RouteData = Encoding.ASCII.GetBytes(value);
                RouteLen = (short)RouteData.Length;
            }
        }

        public string Head
        {
            get => Encoding.ASCII.GetString(HeadData);
            set
            {
                HeadData = Encoding.ASCII.GetBytes(value);
                HeadLen = (short)HeadData.Length;
            }
        }

        public void ToByteArray(ref byte[] target)
        {
            using (var ms = new MemoryStream(target, true))
            {
                using (var writer = new BinaryWriter(ms))
                {
                    writer.Write((byte)CommandId);
                    writer.Write((int)StatusCode);
                    writer.Write(RouteLen);
                    writer.Write(RouteData);
                    writer.Write(HeadLen);
                    writer.Write(HeadData);
                    writer.Write(DataLen);
                }
            }
        }

        public static unsafe Packet FromByteArray(byte[] bytes)
        {
            using (var reader = new BinaryReader(new MemoryStream(bytes)))
            {
                var res = new Packet
                {
                    CommandId =  (Commands)reader.ReadByte(),
                    StatusCode = (HttpStatusCode)reader.ReadInt32(),
                    RouteLen = reader.ReadInt16()
                };
                res.RouteData = reader.ReadBytes(res.RouteLen);
                res.HeadLen = reader.ReadInt16();
                res.HeadData = reader.ReadBytes(res.HeadLen);
                res.DataLen = reader.ReadInt32();
                return res;
            }
        }

        public static Packet FromStruct<T>(string route, Commands cmdId, T contract, Stream data)
        {
            var res = new Packet
            {
                CommandId = cmdId,
                StatusCode = HttpStatusCode.OK,
                Route = route,
                Head = JsonConvert.SerializeObject(contract) ,
                DataLen = data?.Length ?? 0
            };
            //var p = 8 - res.Head.Length % 8;
            res.Head += new string(' ', 500);
            return res;
        }

        public T ToStruct<T>()
        {
            //.Substring(0,Head.Length-4)
            var res = JsonConvert.DeserializeObject<T>(Head);;
            return res;
        }

        public int MySize => MinSize + HeadLen;

        public static readonly int MinSize = SizeOf(typeof(Packet));

        private static int SizeOf(IReflect t)
        {
            var fields = t
                .GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

            return fields.Select(f => f.FieldType)
                .Select(x => x.IsPrimitive ? Marshal.SizeOf(x) : SizeOf(x)).Sum();
        }
    }
}
