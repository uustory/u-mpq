using System.IO;

namespace UFramework
{
    public static class CMD5
    {
        public static string CalculateMD5(Stream stream)
        {
            return CalculateMD5(stream, 64 * 1024);
        }

        public static string CalculateMD5(Stream stream, int bufferSize)
        {
            byte[] _emptyBuffer = new byte[0];
            System.Security.Cryptography.MD5 md5Hasher = System.Security.Cryptography.MD5.Create();
            byte[] buffer = new byte[bufferSize];
            int readBytes;
            while ((readBytes = stream.Read(buffer, 0, bufferSize)) > 0)
            {
                md5Hasher.TransformBlock(buffer, 0, readBytes, buffer, 0);
            }
            md5Hasher.TransformFinalBlock(_emptyBuffer, 0, 0);
            return CUtils.ToHexString(md5Hasher.Hash);
        }
    }
}
