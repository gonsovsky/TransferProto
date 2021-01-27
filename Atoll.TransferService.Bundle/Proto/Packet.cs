using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Atoll.TransferService.Bundle.Proto
{
    public struct Packet
    {
        public HttpStatusCode StatusCode;

        public int RouteLen;

        public byte[] RouteData;

        public int HeadLen;

        public byte[] HeadData;

        public int DataLen;

        public string Route
        {
            get => Encoding.UTF8.GetString(RouteData);
            set
            {
                RouteLen = (short)value.Length;
                RouteData = Encoding.UTF8.GetBytes(value);
            }
        }

        public string Head
        {
            get => Encoding.UTF8.GetString(HeadData);
            set
            {
                HeadLen = (short)value.Length;
                HeadData = Encoding.UTF8.GetBytes(value);
            }
        }

        public void ToByteArray(ref byte[] target)
        {
            using (var ms = new MemoryStream(target, true))
            {
                using (var writer = new BinaryWriter(ms))
                {
                    writer.Write((int)StatusCode);
                    writer.Write(RouteLen);
                    writer.Write(RouteData);
                    writer.Write(HeadLen);
                    writer.Write(Head);
                    writer.Write(DataLen);
                }
            }
        }

        public static Packet FromByteArray(byte[] bytes)
        {
            using (var reader = new BinaryReader(new MemoryStream(bytes)))
            {
                var res = new Packet
                {
                    StatusCode = (HttpStatusCode)reader.ReadInt32(),
                    RouteLen = reader.ReadInt16()
                };
                res.RouteData = reader.ReadBytes(res.RouteLen);
                res.HeadLen = reader.ReadInt32();
                res.HeadData = reader.ReadBytes(res.HeadLen);
                res.DataLen = reader.ReadInt32();
                return res;
            }
        }

        public static Packet FromStruct<T>(string route, T a)
        {
            var res = new Packet
            {
                StatusCode = HttpStatusCode.OK,
                Route = route,
                Head = Newtonsoft.Json.JsonConvert.SerializeObject(a)
            };
            return res;
        }

        public int MySize => MinSize + DataLen + HeadLen;

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
