using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UFramework
{
    /// <summary>
    /// 可读接口
    /// </summary>
    public interface IReadable
    {
        void Read(IInputStream input);
    }

    /// <summary>
    /// 可写接口
    /// </summary>
    public interface IWritable
    {
        void Write(IOutputStream output);
    }

    /// <summary>
    /// 输入流抽象
    /// </summary>
    public interface IInputStream
    {
        byte[] ReadBytes();
        byte[] ReadBytes(int len);
        bool ReadBool();
        byte ReadByte();
        sbyte ReadSByte();
        ushort ReadUShort();
        short ReadShort();
        uint ReadUInt();
        int ReadInt();
        ulong ReadULong();
        long ReadLong();
        float ReadFloat();
        double ReadDouble();
        string ReadString();
    }

    /// <summary>
    /// 输出流抽象
    /// </summary>
    public interface IOutputStream
    {
        void WriteBytes(byte[] bytes);
        void WriteBool(bool value);
        void WriteByte(byte value);
        void WriteSByte(sbyte value);
        void WriteUShort(ushort value);
        void WriteShort(short value);
        void WriteUInt(uint value);
        void WriteInt(int value);
        void WriteULong(ulong value);
        void WriteLong(long value);
        void WriteFloat(float value);
        void WriteDouble(double value);
        void WriteString(string str);
    }


    /// <summary>
    /// 默认的流读取，采用小端方式
    /// </summary>
    public class InputStream : IInputStream
    {
        private Stream stream;

        public InputStream(Stream stream)
        {
            this.stream = stream;
        }

        public void Seek(long offset)
        {
            this.stream.Seek(offset, SeekOrigin.Begin);
        }

        public void Close()
        {
            stream.Close();
        }

        public bool ReadBool()
        {
            return LittleEdian.GetBool(stream);
        }

        public byte[] ReadBytes(int len)
        {
            if (len < 0) return null;
            if (len == 0) return new byte[0];
            byte[] data = new byte[len];
            IOUtil.ReadToEnd(stream, data, 0, len);
            return data;
        }

        public byte[] ReadBytes()
        {
            int len = ReadInt();
            return ReadBytes(len);
        }

        public float ReadFloat()
        {
            return LittleEdian.GetF32(stream);
        }

        public double ReadDouble()
        {
            return LittleEdian.GetF64(stream);
        }

        public short ReadShort()
        {
            return LittleEdian.GetS16(stream);
        }

        public int ReadInt()
        {
            return LittleEdian.GetS32(stream);
        }

        public long ReadLong()
        {
            return LittleEdian.GetS64(stream);
        }

        public sbyte ReadSByte()
        {
            return LittleEdian.GetS8(stream);
        }

        public ushort ReadUShort()
        {
            return LittleEdian.GetU16(stream);
        }

        public uint ReadUInt()
        {
            return LittleEdian.GetU32(stream);
        }

        public ulong ReadULong()
        {
            return LittleEdian.GetU64(stream);
        }

        public byte ReadByte()
        {
            return LittleEdian.GetU8(stream);
        }

        public string ReadString()
        {
            return LittleEdian.GetUTF(stream);
        }
    }

    /// <summary>
    /// 默认的流写出，小端模式
    /// </summary>
    public class OutputStream : IOutputStream
    {
        private Stream stream;

        public OutputStream(Stream output)
        {
            this.stream = output;
        }

        public void WriteBool(bool value)
        {
            LittleEdian.PutBool(stream, value);
        }

        public void WriteBytes(byte[] bytes)
        {
            if (bytes == null) WriteInt(-1);
            else if (bytes.Length == 0) WriteInt(0);
            else
            {
                WriteInt(bytes.Length);
                IOUtil.WriteToEnd(stream, bytes, 0, bytes.Length);
            }
        }

        public void WriteFloat(float value)
        {
            LittleEdian.PutF32(stream, value);
        }

        public void WriteDouble(double value)
        {
            LittleEdian.PutF64(stream, value);
        }

        public void WriteShort(short value)
        {
            LittleEdian.PutS16(stream, value);
        }

        public void WriteInt(int value)
        {
            LittleEdian.PutS32(stream, value);
        }

        public void WriteLong(long value)
        {
            LittleEdian.PutS64(stream, value);
        }

        public void WriteSByte(sbyte value)
        {
            LittleEdian.PutS8(stream, value);
        }

        public void WriteUShort(ushort value)
        {
            LittleEdian.PutU16(stream, value);
        }

        public void WriteUInt(uint value)
        {
            LittleEdian.PutU32(stream, value);
        }

        public void WriteULong(ulong value)
        {
            LittleEdian.PutU64(stream, value);
        }

        public void WriteByte(byte value)
        {
            LittleEdian.PutU8(stream, value);
        }

        public void WriteString(string str)
        {
            LittleEdian.PutUTF(stream, str);
        }
    }

    /// <summary>
    /// 网络输入流
    /// </summary>
    public class NetInputStream : IInputStream
    {
        private Stream stream;

        public NetInputStream(Stream stream)
        {
            this.stream = stream;
        }

        public bool ReadBool()
        {
            return BigEdian.GetBool(stream);
        }

        public byte[] ReadBytes(int len)
        {
            if (len < 0) return null;
            if (len == 0) return new byte[0];
            byte[] data = new byte[len];
            IOUtil.ReadToEnd(stream, data, 0, len);
            return data;
        }

        public byte[] ReadBytes()
        {
            int len = ReadInt();
            return ReadBytes(len);
        }

        public float ReadFloat()
        {
            return BigEdian.GetF32(stream);
        }

        public double ReadDouble()
        {
            return BigEdian.GetF64(stream);
        }

        public short ReadShort()
        {
            return BigEdian.GetS16(stream);
        }

        public int ReadInt()
        {
            return BigEdian.GetS32(stream);
        }

        public long ReadLong()
        {
            return BigEdian.GetS64(stream);
        }

        public sbyte ReadSByte()
        {
            return BigEdian.GetS8(stream);
        }

        public ushort ReadUShort()
        {
            return BigEdian.GetU16(stream);
        }

        public uint ReadUInt()
        {
            return BigEdian.GetU32(stream);
        }

        public ulong ReadULong()
        {
            return BigEdian.GetU64(stream);
        }

        public byte ReadByte()
        {
            return BigEdian.GetU8(stream);
        }

        public string ReadString()
        {
            return BigEdian.GetUTF(stream);
        }
    }

    /// <summary>
    /// 网络输出流
    /// </summary>
    public class NetOutputStream : IOutputStream
    {
        private Stream stream;

        public NetOutputStream(Stream output)
        {
            this.stream = output;
        }

        public void WriteBool(bool value)
        {
            BigEdian.PutBool(stream, value);
        }

        public void WriteBytes(byte[] bytes)
        {
            if (bytes == null) WriteInt(-1);
            else if (bytes.Length == 0) WriteInt(0);
            else
            {
                WriteInt(bytes.Length);
                IOUtil.WriteToEnd(stream, bytes, 0, bytes.Length);
            }
        }

        public void WriteFloat(float value)
        {
            BigEdian.PutF32(stream, value);
        }

        public void WriteDouble(double value)
        {
            BigEdian.PutF64(stream, value);
        }

        public void WriteShort(short value)
        {
            BigEdian.PutS16(stream, value);
        }

        public void WriteInt(int value)
        {
            BigEdian.PutS32(stream, value);
        }

        public void WriteLong(long value)
        {
            BigEdian.PutS64(stream, value);
        }

        public void WriteSByte(sbyte value)
        {
            BigEdian.PutS8(stream, value);
        }

        public void WriteUShort(ushort value)
        {
            BigEdian.PutU16(stream, value);
        }

        public void WriteUInt(uint value)
        {
            BigEdian.PutU32(stream, value);
        }

        public void WriteULong(ulong value)
        {
            BigEdian.PutU64(stream, value);
        }

        public void WriteByte(byte value)
        {
            BigEdian.PutU8(stream, value);
        }

        public void WriteString(string str)
        {
            BigEdian.PutUTF(stream, str);
        }
    }
}