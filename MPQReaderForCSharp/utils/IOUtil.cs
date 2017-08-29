using System.IO;

namespace UFramework
{
    public static class IOUtil
    {
        public static byte[] ReadExpect(Stream input, int count)
        {
            byte[] data = new byte[count];
            int readed = input.Read(data, 0, count);
            while (readed < count)
            {
                readed += input.Read(data, readed, count - readed);
            }
            return data;
        }
        public static void ReadToEnd(Stream input, byte[] data, int offset, int count)
        {
            int readed = input.Read(data, offset, count);
            while (readed < count)
            {
                readed += input.Read(data, offset + readed, count - readed);
            }
        }
        public static void WriteToEnd(Stream output, byte[] data, int offset, int count)
        {
            output.Write(data, offset, count);
        }
    }
}
