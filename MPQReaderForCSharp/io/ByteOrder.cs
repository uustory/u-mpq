using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace UFramework
{
    public static class LittleEdian
    {
        //--------------------------------------------------------------------------------------
        #region BYTES

        public static bool GetBool(byte[] data, ref int pos)
        {
            return data[pos++] != 0;
        }
        public static byte GetU8(byte[] data, ref int pos)
        {
            return data[pos++];
        }
        public static sbyte GetS8(byte[] data, ref int pos)
        {
            return (sbyte)data[pos++];
        }
        public static ushort GetU16(byte[] data, ref int pos)
        {
            int ret = data[pos++];
            ret |= (data[pos++] << 8);
            return (ushort)ret;
        }
        public static short GetS16(byte[] data, ref int pos)
        {
            int ret = data[pos++];
            ret |= (data[pos++] << 8);
            return (short)ret;
        }
        public static uint GetU32(byte[] data, ref int pos)
        {
            uint ret = data[pos++];
            ret |= (uint)(data[pos++] << 8);
            ret |= (uint)(data[pos++] << 16);
            ret |= (uint)(data[pos++] << 24);
            return ret;
        }
        public static int GetS32(byte[] data, ref int pos)
        {
            int ret = data[pos++];
            ret |= ((int)data[pos++] << 8);
            ret |= ((int)data[pos++] << 16);
            ret |= ((int)data[pos++] << 24);
            return ret;
        }
        public static ulong GetU64(byte[] data, ref int pos)
        {
            ulong ret = data[pos++];
            ret |= (((ulong)data[pos++]) << 8);
            ret |= (((ulong)data[pos++]) << 16);
            ret |= (((ulong)data[pos++]) << 24);
            ret |= (((ulong)data[pos++]) << 32);
            ret |= (((ulong)data[pos++]) << 40);
            ret |= (((ulong)data[pos++]) << 48);
            ret |= (((ulong)data[pos++]) << 56);
            return ret;
        }
        public static long GetS64(byte[] data, ref int pos)
        {
            long ret = data[pos++];
            ret |= (((long)data[pos++]) << 8);
            ret |= (((long)data[pos++]) << 16);
            ret |= (((long)data[pos++]) << 24);
            ret |= (((long)data[pos++]) << 32);
            ret |= (((long)data[pos++]) << 40);
            ret |= (((long)data[pos++]) << 48);
            ret |= (((long)data[pos++]) << 56);
            return ret;
        }
        public static string GetUTF(byte[] data, ref int pos)
        {
            int len = GetU16(data, ref pos);
            if (len > 0)
            {
                string ret = System.Text.UTF8Encoding.UTF8.GetString(data, pos, len);
                pos += len;
                return ret;
            }
            return null;
        }
        public static byte[] GetBytes(byte[] data, ref int pos)
        {
            int len = GetU16(data, ref pos);
            byte[] ret = new byte[len];
            if (len > 0)
            {
                Array.Copy(data, pos, ret, 0, len);
                pos += len;
            }
            return ret;
        }

        public static void PutBool(byte[] data, ref int pos, bool value)
        {
            data[pos++] = (byte)(value ? 1 : 0);
        }
        public static void PutU8(byte[] data, ref int pos, byte value)
        {
            data[pos++] = value;
        }
        public static void PutS8(byte[] data, ref int pos, sbyte value)
        {
            data[pos++] = (byte)value;
        }
        public static void PutU16(byte[] data, ref int pos, ushort value)
        {
            data[pos++] = (byte)(value);
            data[pos++] = (byte)(value >> 8);
        }
        public static void PutS16(byte[] data, ref int pos, short value)
        {
            data[pos++] = (byte)(value);
            data[pos++] = (byte)(value >> 8);
        }
        public static void PutU32(byte[] data, ref int pos, uint value)
        {
            data[pos++] = (byte)(value);
            data[pos++] = (byte)(value >> 8);
            data[pos++] = (byte)(value >> 16);
            data[pos++] = (byte)(value >> 24);
        }
        public static void PutS32(byte[] data, ref int pos, int value)
        {
            data[pos++] = (byte)(value);
            data[pos++] = (byte)(value >> 8);
            data[pos++] = (byte)(value >> 16);
            data[pos++] = (byte)(value >> 24);
        }
        public static void PutU64(byte[] data, ref int pos, ulong value)
        {
            data[pos++] = (byte)(value);
            data[pos++] = (byte)(value >> 8);
            data[pos++] = (byte)(value >> 16);
            data[pos++] = (byte)(value >> 24);
            data[pos++] = (byte)(value >> 32);
            data[pos++] = (byte)(value >> 40);
            data[pos++] = (byte)(value >> 48);
            data[pos++] = (byte)(value >> 56);
        }
        public static void PutS64(byte[] data, ref int pos, long value)
        {
            data[pos++] = (byte)(value);
            data[pos++] = (byte)(value >> 8);
            data[pos++] = (byte)(value >> 16);
            data[pos++] = (byte)(value >> 24);
            data[pos++] = (byte)(value >> 32);
            data[pos++] = (byte)(value >> 40);
            data[pos++] = (byte)(value >> 48);
            data[pos++] = (byte)(value >> 56);
        }
        public static void PutUTF(byte[] data, ref int pos, string value)
        {
            if (value == null || value.Length == 0)
            {
                PutU16(data, ref pos, 0);
            }
            else
            {
                byte[] buff = System.Text.UTF8Encoding.UTF8.GetBytes(value);
                if (buff.Length > UInt16.MaxValue)
                {
                    throw new IOException("PutUTF overflow : " + value + "\nSize=" + buff.Length);
                }
                PutU16(data, ref pos, (ushort)buff.Length);
                Put(data, ref pos, buff, 0, buff.Length);
            }
        }
        public static void PutBytes(byte[] data, ref int pos, byte[] value)
        {
            if (value == null || value.Length == 0)
            {
                PutS32(data, ref pos, 0);
            }
            else
            {
                PutS32(data, ref pos, value.Length);
                Put(data, ref pos, value, 0, value.Length);
            }
        }
        public static void Get(byte[] data, ref int pos, byte[] value, int offset, int length)
        {
            Array.Copy(data, pos, value, offset, length);
            pos += length;
        }
        public static void Put(byte[] data, ref int pos, byte[] value, int offset, int length)
        {
            Array.Copy(value, offset, data, pos, length);
            pos += length;
        }

        #endregion
        //--------------------------------------------------------------------------------------
        #region STREAM

        public static bool GetBool(Stream data)
        {
            return data.ReadByte() != 0;
        }
        public static void PutBool(Stream data, bool value)
        {
            data.WriteByte((byte)(value ? 1 : 0));
        }

        public static byte GetU8(Stream data)
        {
            return (byte)data.ReadByte();
        }
        public static void PutU8(Stream data, byte value)
        {
            data.WriteByte(value);
        }

        public static sbyte GetS8(Stream data)
        {
            return (sbyte)data.ReadByte();
        }
        public static void PutS8(Stream data, sbyte value)
        {
            data.WriteByte((byte)value);
        }

        public static ushort GetU16(Stream data)
        {
            int ret = (data.ReadByte());
            ret |= (data.ReadByte() << 8);
            return (ushort)ret;
        }
        public static void PutU16(Stream data, ushort value)
        {
            data.WriteByte((byte)(value));
            data.WriteByte((byte)(value >> 8));
        }

        public static short GetS16(Stream data)
        {
            int ret = (data.ReadByte());
            ret |= (data.ReadByte() << 8);
            return (short)ret;
        }
        //------------------------------------------------------------------------------------------

        public static void PutS16(Stream data, short value)
        {
            data.WriteByte((byte)(value));
            data.WriteByte((byte)(value >> 8));
        }
        public static uint GetU32(Stream data)
        {
            int ret = data.ReadByte();
            ret |= (data.ReadByte() << 8);
            ret |= (data.ReadByte() << 16);
            ret |= (data.ReadByte() << 24);
            return (uint)ret;
        }
        public static void PutU32(Stream data, uint value)
        {
            data.WriteByte((byte)(value));
            data.WriteByte((byte)(value >> 8));
            data.WriteByte((byte)(value >> 16));
            data.WriteByte((byte)(value >> 24));
        }
        public static int GetS32(Stream data)
        {
            int ret = data.ReadByte();
            ret |= (data.ReadByte() << 8);
            ret |= (data.ReadByte() << 16);
            ret |= (data.ReadByte() << 24);
            return ret;
        }
        public static void PutS32(Stream data, int value)
        {
            data.WriteByte((byte)(value));
            data.WriteByte((byte)(value >> 8));
            data.WriteByte((byte)(value >> 16));
            data.WriteByte((byte)(value >> 24));
        }
        public static ulong GetU64(Stream data)
        {
            ulong ret = (ulong)(data.ReadByte());
            ret |= (((ulong)data.ReadByte()) << 8);
            ret |= (((ulong)data.ReadByte()) << 16);
            ret |= (((ulong)data.ReadByte()) << 24);
            ret |= (((ulong)data.ReadByte()) << 32);
            ret |= (((ulong)data.ReadByte()) << 40);
            ret |= (((ulong)data.ReadByte()) << 48);
            ret |= (((ulong)data.ReadByte()) << 56);
            return ret;
        }
        public static void PutU64(Stream data, ulong value)
        {
            data.WriteByte((byte)(value));
            data.WriteByte((byte)(value >> 8));
            data.WriteByte((byte)(value >> 16));
            data.WriteByte((byte)(value >> 24));
            data.WriteByte((byte)(value >> 32));
            data.WriteByte((byte)(value >> 40));
            data.WriteByte((byte)(value >> 48));
            data.WriteByte((byte)(value >> 56));
        }
        public static long GetS64(Stream data)
        {
            long ret = data.ReadByte();
            ret |= (((long)data.ReadByte()) << 8);
            ret |= (((long)data.ReadByte()) << 16);
            ret |= (((long)data.ReadByte()) << 24);
            ret |= (((long)data.ReadByte()) << 32);
            ret |= (((long)data.ReadByte()) << 40);
            ret |= (((long)data.ReadByte()) << 48);
            ret |= (((long)data.ReadByte()) << 56);
            return ret;
        }

        public static void PutS64(Stream data, long value)
        {
            data.WriteByte((byte)(value));
            data.WriteByte((byte)(value >> 8));
            data.WriteByte((byte)(value >> 16));
            data.WriteByte((byte)(value >> 24));
            data.WriteByte((byte)(value >> 32));
            data.WriteByte((byte)(value >> 40));
            data.WriteByte((byte)(value >> 48));
            data.WriteByte((byte)(value >> 56));
        }

        //--------------------------------------------------------------------------------------

        public static float GetF32(Stream data)
        {
            byte[] buff = IOUtil.ReadExpect(data, 4);
            return System.BitConverter.ToSingle(buff, 0);
        }
        public static void PutF32(Stream data, float value)
        {
            byte[] buff = BitConverter.GetBytes(value);
            data.Write(buff, 0, 4);
        }
        public static double GetF64(Stream data)
        {
            byte[] buff = IOUtil.ReadExpect(data, 4);
            return System.BitConverter.ToDouble(buff, 0);
        }
        public static void PutF64(Stream data, double value)
        {
            byte[] buff = BitConverter.GetBytes(value);
            data.Write(buff, 0, 8);
        }

        //--------------------------------------------------------------------------------------

        public static string GetUTF(Stream data)
        {
            int len = GetU16(data);
            if (len > 0)
            {
                byte[] buff = IOUtil.ReadExpect(data, len);
                return System.Text.UTF8Encoding.UTF8.GetString(buff, 0, len);
            }
            return null;
        }
        public static void PutUTF(Stream data, string str)
        {
            if (str == null || str.Length == 0)
            {
                PutU16(data, 0);
            }
            else
            {
                byte[] buff = System.Text.UTF8Encoding.UTF8.GetBytes(str);
                if (buff.Length > UInt16.MaxValue)
                {
                    throw new IOException("PutUTF overflow : " + str + "\nSize=" + buff.Length);
                }
                PutU16(data, (ushort)buff.Length);
                data.Write(buff, 0, buff.Length);
            }
        }

        //--------------------------------------------------------------------------------------

        public static T GetEnum8<T>(Stream data, Type enumType)
        {
            return (T)Enum.ToObject(enumType, data.ReadByte());
        }

        public static void PutEnum8(Stream data, object enumData)
        {
            byte b = (byte)(enumData);
            data.WriteByte(b);
        }

        //--------------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------------

        public delegate T GetData<T>(Stream data);
        public delegate void PutData<T>(Stream data, T v);

        public static T[] GetArray<T>(Stream data, GetData<T> action) where T : new()
        {
            int len = GetU16(data);
            T[] ret = new T[len];
            for (int i = 0; i < len; i++)
            {
                T d = action.Invoke(data);
                ret[i] = d;
            }
            return ret;
        }
        public static void PutArray<T>(Stream data, T[] array, PutData<T> action)
        {
            if (array == null)
            {
                PutU16(data, 0);
            }
            else
            {
                int len = array.Length;
                if (len > UInt16.MaxValue)
                {
                    throw new IOException("PutArray overflow : " + array + "\nSize=" + len);
                }
                PutU16(data, (ushort)len);
                for (int i = 0; i < len; i++)
                {
                    action.Invoke(data, array[i]);
                }
            }
        }

        public static List<T> GetList<T>(Stream data, GetData<T> action) where T : new()
        {
            int len = GetU16(data);
            List<T> ret = new List<T>(len);
            for (int i = 0; i < len; i++)
            {
                T d = action.Invoke(data);
                ret.Add(d);
            }
            return ret;
        }

        public static void PutList<T>(Stream data, List<T> list, PutData<T> action)
        {
            if (list == null)
            {
                PutU16(data, 0);
            }
            else
            {
                int len = list.Count;
                if (len > UInt16.MaxValue)
                {
                    throw new IOException("PutList overflow : " + list + "\nSize=" + len);
                }
                PutU16(data, (ushort)len);
                for (int i = 0; i < len; i++)
                {
                    action.Invoke(data, list[i]);
                }
            }
        }

        #endregion
        //--------------------------------------------------------------------------------------
    }


    public static class BigEdian
    {
        //--------------------------------------------------------------------------------------
        #region BYTES

        public static bool GetBool(byte[] data, ref int pos)
        {
            return data[pos++] != 0;
        }
        public static byte GetU8(byte[] data, ref int pos)
        {
            return data[pos++];
        }
        public static sbyte GetS8(byte[] data, ref int pos)
        {
            return (sbyte)data[pos++];
        }
        public static ushort GetU16(byte[] data, ref int pos)
        {
            int ret = 0;
            ret |= (data[pos++] << 8);
            ret |= (data[pos++]);
            return (ushort)ret;
        }
        public static short GetS16(byte[] data, ref int pos)
        {
            int ret = 0;
            ret |= (data[pos++] << 8);
            ret |= (data[pos++]);
            return (short)ret;
        }
        public static uint GetU32(byte[] data, ref int pos)
        {
            uint ret = 0;
            ret |= (uint)(data[pos++] << 24);
            ret |= (uint)(data[pos++] << 16);
            ret |= (uint)(data[pos++] << 8);
            ret |= (uint)(data[pos++]);
            return ret;
        }
        public static int GetS32(byte[] data, ref int pos)
        {
            int ret = 0;
            ret |= ((int)data[pos++] << 24);
            ret |= ((int)data[pos++] << 16);
            ret |= ((int)data[pos++] << 8);
            ret |= ((int)data[pos++]);
            return ret;
        }
        public static ulong GetU64(byte[] data, ref int pos)
        {
            ulong ret = 0;
            ret |= (((ulong)data[pos++]) << 56);
            ret |= (((ulong)data[pos++]) << 48);
            ret |= (((ulong)data[pos++]) << 40);
            ret |= (((ulong)data[pos++]) << 32);
            ret |= (((ulong)data[pos++]) << 24);
            ret |= (((ulong)data[pos++]) << 16);
            ret |= (((ulong)data[pos++]) << 8);
            ret |= (((ulong)data[pos++]));
            return ret;
        }
        public static long GetS64(byte[] data, ref int pos)
        {
            long ret = 0;
            ret |= (((long)data[pos++]) << 56);
            ret |= (((long)data[pos++]) << 48);
            ret |= (((long)data[pos++]) << 40);
            ret |= (((long)data[pos++]) << 32);
            ret |= (((long)data[pos++]) << 24);
            ret |= (((long)data[pos++]) << 16);
            ret |= (((long)data[pos++]) << 8);
            ret |= (((long)data[pos++]));
            return ret;
        }
        public static string GetUTF(byte[] data, ref int pos)
        {
            int len = GetU16(data, ref pos);
            if (len > 0)
            {
                string ret = System.Text.UTF8Encoding.UTF8.GetString(data, pos, len);
                pos += len;
                return ret;
            }
            return null;
        }
        public static byte[] GetBytes(byte[] data, ref int pos)
        {
            int len = GetU16(data, ref pos);
            byte[] ret = new byte[len];
            if (len > 0)
            {
                Array.Copy(data, pos, ret, 0, len);
                pos += len;
            }
            return ret;
        }

        public static void PutBool(byte[] data, ref int pos, bool value)
        {
            data[pos++] = (byte)(value ? 1 : 0);
        }
        public static void PutU8(byte[] data, ref int pos, byte value)
        {
            data[pos++] = value;
        }
        public static void PutS8(byte[] data, ref int pos, sbyte value)
        {
            data[pos++] = (byte)value;
        }
        public static void PutU16(byte[] data, ref int pos, ushort value)
        {
            data[pos++] = (byte)(value >> 8);
            data[pos++] = (byte)(value);
        }
        public static void PutS16(byte[] data, ref int pos, short value)
        {
            data[pos++] = (byte)(value >> 8);
            data[pos++] = (byte)(value);
        }
        public static void PutU32(byte[] data, ref int pos, uint value)
        {
            data[pos++] = (byte)(value >> 24);
            data[pos++] = (byte)(value >> 16);
            data[pos++] = (byte)(value >> 8);
            data[pos++] = (byte)(value);
        }
        public static void PutS32(byte[] data, ref int pos, int value)
        {
            data[pos++] = (byte)(value >> 24);
            data[pos++] = (byte)(value >> 16);
            data[pos++] = (byte)(value >> 8);
            data[pos++] = (byte)(value);
        }
        public static void PutU64(byte[] data, ref int pos, ulong value)
        {
            data[pos++] = (byte)(value >> 56);
            data[pos++] = (byte)(value >> 48);
            data[pos++] = (byte)(value >> 40);
            data[pos++] = (byte)(value >> 32);
            data[pos++] = (byte)(value >> 24);
            data[pos++] = (byte)(value >> 16);
            data[pos++] = (byte)(value >> 8);
            data[pos++] = (byte)(value);
        }
        public static void PutS64(byte[] data, ref int pos, long value)
        {
            data[pos++] = (byte)(value >> 56);
            data[pos++] = (byte)(value >> 48);
            data[pos++] = (byte)(value >> 40);
            data[pos++] = (byte)(value >> 32);
            data[pos++] = (byte)(value >> 24);
            data[pos++] = (byte)(value >> 16);
            data[pos++] = (byte)(value >> 8);
            data[pos++] = (byte)(value);
        }
        public static void PutUTF(byte[] data, ref int pos, string value)
        {
            if (value == null || value.Length == 0)
            {
                PutU16(data, ref pos, 0);
            }
            else
            {
                byte[] buff = System.Text.UTF8Encoding.UTF8.GetBytes(value);
                if (buff.Length > UInt16.MaxValue)
                {
                    throw new IOException("PutUTF overflow : " + value + "\nSize=" + buff.Length);
                }
                PutU16(data, ref pos, (ushort)buff.Length);
                Put(data, ref pos, buff, 0, buff.Length);
            }
        }
        public static void PutBytes(byte[] data, ref int pos, byte[] value)
        {
            if (value == null || value.Length == 0)
            {
                PutS32(data, ref pos, 0);
            }
            else
            {
                PutS32(data, ref pos, value.Length);
                Put(data, ref pos, value, 0, value.Length);
            }
        }
        public static void Get(byte[] data, ref int pos, byte[] value, int offset, int length)
        {
            Array.Copy(data, pos, value, offset, length);
            pos += length;
        }
        public static void Put(byte[] data, ref int pos, byte[] value, int offset, int length)
        {
            Array.Copy(value, offset, data, pos, length);
            pos += length;
        }

        #endregion
        //--------------------------------------------------------------------------------------
        #region STREAM

        public static bool GetBool(Stream data)
        {
            return data.ReadByte() != 0;
        }
        public static void PutBool(Stream data, bool value)
        {
            data.WriteByte((byte)(value ? 1 : 0));
        }

        public static byte GetU8(Stream data)
        {
            return (byte)data.ReadByte();
        }
        public static void PutU8(Stream data, byte value)
        {
            data.WriteByte(value);
        }

        public static sbyte GetS8(Stream data)
        {
            return (sbyte)data.ReadByte();
        }
        public static void PutS8(Stream data, sbyte value)
        {
            data.WriteByte((byte)value);
        }

        public static ushort GetU16(Stream data)
        {
            int ret = 0;
            ret |= (data.ReadByte() << 8);
            ret |= (data.ReadByte());
            return (ushort)ret;
        }
        public static void PutU16(Stream data, ushort value)
        {
            data.WriteByte((byte)(value >> 8));
            data.WriteByte((byte)(value));
        }

        public static short GetS16(Stream data)
        {
            int ret = 0;
            ret |= (data.ReadByte() << 8);
            ret |= (data.ReadByte());
            return (short)ret;
        }
        public static void PutS16(Stream data, short value)
        {
            data.WriteByte((byte)(value >> 8));
            data.WriteByte((byte)(value));
        }

        public static uint GetU32(Stream data)
        {
            int ret = 0;
            ret |= (data.ReadByte() << 24);
            ret |= (data.ReadByte() << 16);
            ret |= (data.ReadByte() << 8);
            ret |= (data.ReadByte());
            return (uint)ret;
        }
        public static void PutU32(Stream data, uint value)
        {
            data.WriteByte((byte)(value >> 24));
            data.WriteByte((byte)(value >> 16));
            data.WriteByte((byte)(value >> 8));
            data.WriteByte((byte)(value));
        }
        public static int GetS32(Stream data)
        {
            int ret = 0;
            ret |= (data.ReadByte() << 24);
            ret |= (data.ReadByte() << 16);
            ret |= (data.ReadByte() << 8);
            ret |= (data.ReadByte());
            return ret;
        }
        public static void PutS32(Stream data, int value)
        {
            data.WriteByte((byte)(value >> 24));
            data.WriteByte((byte)(value >> 16));
            data.WriteByte((byte)(value >> 8));
            data.WriteByte((byte)(value));
        }
        public static ulong GetU64(Stream data)
        {
            ulong ret = 0;
            ret |= (((ulong)data.ReadByte()) << 56);
            ret |= (((ulong)data.ReadByte()) << 48);
            ret |= (((ulong)data.ReadByte()) << 40);
            ret |= (((ulong)data.ReadByte()) << 32);
            ret |= (((ulong)data.ReadByte()) << 24);
            ret |= (((ulong)data.ReadByte()) << 16);
            ret |= (((ulong)data.ReadByte()) << 8);
            ret |= (((ulong)data.ReadByte()) << 0);
            return ret;
        }
        public static void PutU64(Stream data, ulong value)
        {
            data.WriteByte((byte)(value >> 56));
            data.WriteByte((byte)(value >> 48));
            data.WriteByte((byte)(value >> 40));
            data.WriteByte((byte)(value >> 32));
            data.WriteByte((byte)(value >> 24));
            data.WriteByte((byte)(value >> 16));
            data.WriteByte((byte)(value >> 8));
            data.WriteByte((byte)(value));
        }
        public static long GetS64(Stream data)
        {
            long ret = 0;
            ret |= (((long)data.ReadByte()) << 56);
            ret |= (((long)data.ReadByte()) << 48);
            ret |= (((long)data.ReadByte()) << 40);
            ret |= (((long)data.ReadByte()) << 32);
            ret |= (((long)data.ReadByte()) << 24);
            ret |= (((long)data.ReadByte()) << 16);
            ret |= (((long)data.ReadByte()) << 8);
            ret |= (((long)data.ReadByte()) << 0);
            return ret;
        }

        public static void PutS64(Stream data, long value)
        {
            data.WriteByte((byte)(value >> 56));
            data.WriteByte((byte)(value >> 48));
            data.WriteByte((byte)(value >> 40));
            data.WriteByte((byte)(value >> 32));
            data.WriteByte((byte)(value >> 24));
            data.WriteByte((byte)(value >> 16));
            data.WriteByte((byte)(value >> 8));
            data.WriteByte((byte)(value));
        }

        //--------------------------------------------------------------------------------------

        public static float GetF32(Stream data)
        {
            byte[] buff = IOUtil.ReadExpect(data, 4);
            return System.BitConverter.ToSingle(buff, 0);
        }
        public static void PutF32(Stream data, float value)
        {
            byte[] buff = BitConverter.GetBytes(value);
            data.Write(buff, 0, 4);
        }
        public static double GetF64(Stream data)
        {
            byte[] buff = IOUtil.ReadExpect(data, 8);
            return System.BitConverter.ToDouble(buff, 0);
        }
        public static void PutF64(Stream data, double value)
        {
            byte[] buff = BitConverter.GetBytes(value);
            data.Write(buff, 0, 8);
        }

        //--------------------------------------------------------------------------------------

        public static string GetUTF(Stream data)
        {
            int len = GetU16(data);
            if (len > 0)
            {
                byte[] buff = IOUtil.ReadExpect(data, len);
                return System.Text.UTF8Encoding.UTF8.GetString(buff, 0, len);
            }
            return null;
        }
        public static void PutUTF(Stream data, string str)
        {
            if (str == null || str.Length == 0)
            {
                PutU16(data, 0);
            }
            else
            {
                byte[] buff = System.Text.UTF8Encoding.UTF8.GetBytes(str);
                if (buff.Length > UInt16.MaxValue)
                {
                    throw new IOException("PutUTF overflow : " + str + "\nSize=" + buff.Length);
                }
                PutU16(data, (ushort)buff.Length);
                data.Write(buff, 0, buff.Length);
            }
        }

        //--------------------------------------------------------------------------------------

        public static T GetEnum8<T>(Stream data, Type enumType)
        {
            return (T)Enum.ToObject(enumType, data.ReadByte());
        }

        public static void PutEnum8(Stream data, object enumData)
        {
            byte b = (byte)(enumData);
            data.WriteByte(b);
        }

        //--------------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------------

        public delegate T GetData<T>(Stream data);
        public delegate void PutData<T>(Stream data, T v);

        public static T[] GetArray<T>(Stream data, GetData<T> action) where T : new()
        {
            int len = GetU16(data);
            T[] ret = new T[len];
            for (int i = 0; i < len; i++)
            {
                T d = action.Invoke(data);
                ret[i] = d;
            }
            return ret;
        }
        public static void PutArray<T>(Stream data, T[] array, PutData<T> action)
        {
            if (array == null)
            {
                PutU16(data, 0);
            }
            else
            {
                int len = array.Length;
                if (len > UInt16.MaxValue)
                {
                    throw new IOException("PutArray overflow : " + array + "\nSize=" + len);
                }
                PutU16(data, (ushort)len);
                for (int i = 0; i < len; i++)
                {
                    action.Invoke(data, array[i]);
                }
            }
        }

        public static List<T> GetList<T>(Stream data, GetData<T> action) where T : new()
        {
            int len = GetU16(data);
            List<T> ret = new List<T>(len);
            for (int i = 0; i < len; i++)
            {
                T d = action.Invoke(data);
                ret.Add(d);
            }
            return ret;
        }

        public static void PutList<T>(Stream data, List<T> list, PutData<T> action)
        {
            if (list == null)
            {
                PutU16(data, 0);
            }
            else
            {
                int len = list.Count;
                if (len > UInt16.MaxValue)
                {
                    throw new IOException("PutList overflow : " + list + "\nSize=" + len);
                }
                PutU16(data, (ushort)len);
                for (int i = 0; i < len; i++)
                {
                    action.Invoke(data, list[i]);
                }
            }
        }

        #endregion
        //--------------------------------------------------------------------------------------
    }



}
