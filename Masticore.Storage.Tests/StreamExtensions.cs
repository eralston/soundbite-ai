using System.IO;
using System.Text;

namespace Masticore.Storage.Tests
{
    public static class StreamExtensions
    {
        public static Stream ToStream(this string val)
        {
            byte[] byteArray = Encoding.ASCII.GetBytes(val);
            MemoryStream stream = new MemoryStream(byteArray);
            return stream;
        }

        public static string ToText(this Stream stream)
        {
            StreamReader reader = new StreamReader(stream);
            string text = reader.ReadToEnd();
            return text;
        }
    }
}
