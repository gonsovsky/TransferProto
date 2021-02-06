using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace TestClient
{
    public class RecvPacket : IDisposable
    {
        public HttpStatusCode StatusCode;

        public int BodyLen;

        public byte[] BodyData;

        public static RecvPacket FromByteArray(byte[] bytes, int cnt)
        {
            using (var reader = new BinaryReader(new MemoryStream(bytes)))
            {
                var res = new RecvPacket
                {
                    StatusCode = (HttpStatusCode)reader.ReadInt32(),
                    BodyLen = reader.ReadInt32()
                };
                res.BodyData = reader.ReadBytes(res.BodyLen);
               
                return res;
            }
        }

        private string body;
        public string Body
        {
            get => body ?? (body = Encoding.UTF8.GetString(BodyData));
            set
            {
                if (value == body)
                    return;
                BodyData = Encoding.UTF8.GetBytes(value);
                BodyLen = (short)BodyData.Length;
            }
        }


        public void Dispose()
        {
        }


        public int MySize => MinSize + BodyLen;

        public static readonly int MinSize = SizeOf(typeof(RecvPacket));

        private static int SizeOf(IReflect t)
        {
            var fields = t
                .GetFields(BindingFlags.Public | BindingFlags.Instance);

            return fields.Select(f => f.FieldType)
                .Select(x => x.IsPrimitive ? Marshal.SizeOf(x) : SizeOf(x)).Sum();
        }
    }

    public class SendPacket: IDisposable
    {
        public int RouteLen;

        public byte[] RouteData;

        public int BodyLen;

        public byte[] BodyData;

        public long Offset;

        public long Length;

        public long PostLen;

        private string route;
        public string Route
        {
            get => route ?? (route = Encoding.UTF8.GetString(RouteData));
            set
            {
                if (value == route)
                    return;
                RouteData = Encoding.UTF8.GetBytes(value);
                RouteLen = (short)RouteData.Length;
            }
        }

        private string body;
        public string Body
        {
            get => body ?? (body = Encoding.UTF8.GetString(BodyData));
            set
            {
                if (value == body)
                    return;
                BodyData = Encoding.UTF8.GetBytes(value);
                BodyLen = (short)BodyData.Length;
            }
        }

        public void ToByteArray(ref byte[] target)
        {
            using (var ms = new MemoryStream(target, true))
            {
                using (var writer = new BinaryWriter(ms))
                {
                    writer.Write(RouteLen);
                    writer.Write(RouteData);
                    writer.Write(BodyLen);
                    writer.Write(BodyData);
                    writer.Write(Offset);
                    writer.Write(Length);
                    writer.Write(PostLen);
                }
            }
        }

        public int MySize => MinSize + BodyLen;

        public static readonly int MinSize = SizeOf(typeof(SendPacket));

        private static int SizeOf(IReflect t)
        {
            var fields = t
                .GetFields(BindingFlags.Public | BindingFlags.Instance);

            return fields.Select(f => f.FieldType)
                .Select(x => x.IsPrimitive ? Marshal.SizeOf(x) : SizeOf(x)).Sum();
        }

        public void Dispose()
        {
        }
    }
}
