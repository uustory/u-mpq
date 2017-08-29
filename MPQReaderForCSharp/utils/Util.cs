using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Collections;

namespace UFramework
{
    public static class CUtils
    {

        public static Encoding UTF8 = new UTF8Encoding(false);

        public static long CurrentTimeMS
        {
            get { return System.DateTime.Now.Ticks / 10000; }
        }

        /// <summary>
        /// 将路径修改为左斜杠格式，同时将两边所有的左边和右边所有的左斜杠删除
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string TrimPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return "";
            }

            path = path.Replace("\\", "/");

            while (path.StartsWith("/"))
                path = path.Substring(1);

            while(path.EndsWith("/"))
                path = path.Substring(0, path.Length - 1);

            return path;
        }

        /// <summary>
        /// 循环创建目录
        /// </summary>
        /// <param name="dir"></param>
        public static void CreateDir(DirectoryInfo dir)
        {
            if (dir.Exists)
            {
                return;
            }

            DirectoryInfo parent = dir.Parent;
            if (!parent.Exists)
            {
                CreateDir(parent);
            }

            dir.Create();
        }

        /// <summary>
        /// 删除目录下的所有空子目录
        /// </summary>
        /// <param name="dir"></param>
        public static void RemoteEmptyDir(DirectoryInfo dir)
        {
            var subDirs = dir.GetDirectories("*.*", SearchOption.AllDirectories);
            foreach (var d in subDirs)
            {
                if (d.GetFileSystemInfos().Count() == 0)
                {
                    d.Delete();
                }
            }
        }


        public static IntPtr ToIntPtr(object obj)
        {
            if (obj == null)
            {
                return new IntPtr(0);
            }
            GCHandle hObject = GCHandle.Alloc(obj, GCHandleType.Pinned);
            IntPtr pObject = hObject.AddrOfPinnedObject();
            if (hObject.IsAllocated)
                hObject.Free();
            return pObject;
        }

        public static T ToEnum<T>(object value)
        {
            if (Enum.IsDefined(typeof(T), value))
            {
                return (T)value;
            }
            return default(T);
        }

        public static bool RandomPercent(Random random, float percent)
        {
            //大于等于 0.0 并且小于 1.0 的双精度浮点数。//
            if (random.NextDouble() * 100f < percent)
            {
                return true;
            }
            return false;
        }


        #region CLONE

        public static T TryClone<T>(T src) where T : class, ICloneable
        {
            if (src == null) return null;
            return (T)src.Clone();
        }

        public static T[] CloneArray<T>(T[] src)
            where T : class, ICloneable
        {
            if (src == null) return null;
            T[] ret = new T[src.Length];
            for (int i = 0; i < src.Length; i++)
            {
                if (src[i] != null)
                {
                    ret[i] = (T)src[i].Clone();
                }
                else
                {
                    ret[i] = null;
                }
            }
            return ret;
        }

        public static List<T> CloneList<T>(IList<T> src)
            where T : class, ICloneable
        {
            if (src == null) return null;
            List<T> ret = new List<T>(src.Count);
            for (int i = 0; i < src.Count; i++)
            {
                if (src[i] != null)
                {
                    ret.Add((T)src[i].Clone());
                }
                else
                {
                    ret.Add(null);
                }
            }
            return ret;
        }

        public static Dictionary<K, V> CloneMap<K, V>(IDictionary<K, V> map)
            where V : ICloneable
        {
            if (map == null) return null;
            Dictionary<K, V> ret = new Dictionary<K, V>(map.Count);
            foreach (K k in map.Keys)
            {
                ret.Add(k, (V)ret[k].Clone());
            }
            return ret;
        }

        #endregion

        #region STRING

        public static string ToHexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                byte d = data[i];
                sb.Append(d.ToString("x2"));
            }
            return sb.ToString();
        }

        public static string BinToHex(byte[] bin)
        {
            StringBuilder hexdata = new StringBuilder();
            for (int i = 0; i < bin.Length; i++)
            {
                string hex = bin[i].ToString("X2");
                if (hex.Length < 2)
                {
                    hexdata.Append("0" + hex);
                }
                else if (hex.Length == 2)
                {
                    hexdata.Append(hex);
                }
            }
            return hexdata.ToString();
        }

        public static byte[] HexToBin(string hex)
        {
            if (hex.Length % 2 != 0)
            {
                hex = "0" + hex;
            }
            int count = hex.Length;
            byte[] os = new byte[count / 2 + 1];
            for (int i = 0; i < count; i += 2)
            {
                string hch = hex.Substring(i, 2);
                byte read = byte.Parse(hch, NumberStyles.AllowHexSpecifier);
                os[i / 2] = read;
            }
            return os;
        }


        public static string ListToString(IList list, string split = ", ")
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                object obj = list[i];
                sb.Append(obj + "");
                if (i < list.Count - 1)
                {
                    sb.Append(split);
                }
            }
            return sb.ToString();
        }


        public static string MapToString(IDictionary list, string kv_split = "=", string line_split = "\n")
        {
            StringBuilder sb = new StringBuilder();
            int i = 0;
            foreach (object key in list.Keys)
            {
                object obj = list[key];
                sb.Append(key + kv_split + obj);
                if (i < list.Count - 1)
                {
                    sb.Append(line_split);
                }
                i++;
            }
            return sb.ToString();
        }


        public static string ArrayToString(Array list, string split = ", ")
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < list.Length; i++)
            {
                object obj = list.GetValue(i);
                sb.Append(obj + "");
                if (i < list.Length - 1)
                {
                    sb.Append(split);
                }
            }
            return sb.ToString();
        }



        /// <summary>
        /// 生成一个倒计时用的时间字符串.
        /// 根据输入的秒数生成字符串 eg:60 为 60秒. 
        ///格式：.
        /// DD: 总天数.
        /// hh：小时余数（除去天数).
        /// HH: 总小时数.
        /// mm: 分钟余数（除去小时）.
        /// MM: 总分钟数.
        /// ss：秒数（除去分钟）.
        /// @Example:.
        /// trace(formatTime(5000, '[MM]:[ss]'));.
        ///Will output:.
        /// [83]:[20].
        /// trace(formatTime(5000, '[mm]:[ss]'));.
        /// Will output:.
        /// [23]:[20].
        /// @param ts_second      要进行格式化的秒数.
        /// @param format         返回的格式 (默认为 HH:mm:ss ).
        /// @param isFullFormat   当数字小于10时是否在前面补0 (默认是补的 即 true).
        /// </summary>
        /// <param name="random"></param>
        /// <param name="percent"></param>
        /// <returns></returns>

        public static string FormatTime(uint ts_second, string format = "HH:mm:ss", bool isFullFormat = true)
        {

            uint DD;

            uint hh;

            uint HH;

            uint mm;

            uint MM;

            uint ss;

            DD = (uint)Math.Floor((double)(ts_second / 86400));

            hh = (uint)Math.Floor((double)(ts_second - DD * 86400) / 3600);

            HH = (uint)Math.Floor((double)ts_second / 3600);

            mm = (uint)Math.Floor((double)(ts_second - HH * 3600) / 60);

            MM = (uint)Math.Floor((double)ts_second / 60);

            ss = ts_second - MM * 60;


            string ret = format;


            ret = ret.Replace("DD", isFullFormat && DD < 10 ? "0" + DD : DD + "");

            ret = ret.Replace("hh", isFullFormat && hh < 10 ? "0" + hh : hh + "");

            ret = ret.Replace("HH", isFullFormat && HH < 10 ? "0" + HH : HH + "");

            ret = ret.Replace("mm", isFullFormat && mm < 10 ? "0" + mm : mm + "");

            ret = ret.Replace("MM", isFullFormat && MM < 10 ? "0" + MM : MM + "");

            ret = ret.Replace("ss", isFullFormat && ss < 10 ? "0" + ss : ss + "");

            return ret;

        }

        #endregion


        #region ARRAY_AND_COLLECTIONS

        public static IList<T> RemoveAll<T>(IList<T> src, ICollection<T> list)
        {
            for (int i = src.Count - 1; i >= 0; i--)
            {
                T e = src[i];
                if (e != null && list.Contains(e))
                {
                    src.RemoveAt(i);
                }
            }
            return src;
        }

        public static void RandomList<T>(Random random, IList<T> src)
        {
            for (int i = src.Count - 1; i >= 0; i--)
            {
                int r = random.Next(0, src.Count);
                T t = src[r];
                src[r] = src[i];
                src[i] = t;
            }
        }
        public static void RandomArray<T>(Random random, T[] src)
        {
            for (int i = src.Length - 1; i >= 0; i--)
            {
                int r = random.Next(0, src.Length);
                T t = src[r];
                src[r] = src[i];
                src[i] = t;
            }
        }
        public static T[] ArrayLink<T>(T[] a, params T[] b)
        {
            T[] dst = new T[a.Length + b.Length];
            Array.Copy(a, 0, dst, 0, a.Length);
            Array.Copy(b, 0, dst, a.Length, b.Length);
            return dst;
        }

        public static void ArrayCopy<T>(ICollection<T> src, Queue<T> dst)
        {
            foreach (T t in src)
            {
                dst.Enqueue(t);
            }
        }
        public static void ArrayCopy<T>(ICollection<T> src, ICollection<T> dst)
        {
            foreach (T t in src)
            {
                dst.Add(t);
            }
        }

        public static bool ArraysEqual<T>(T[] a1, T[] a2)
        {
            if (ReferenceEquals(a1, a2))
                return true;

            if (a1 == null || a2 == null)
                return false;

            if (a1.Length != a2.Length)
                return false;

            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < a1.Length; i++)
            {
                if (!comparer.Equals(a1[i], a2[i])) return false;
            }
            return true;
        }

        public static T GetRandomInArray<T>(IList<T> list, Random random)
        {
            int rd = random.Next(list.Count);
            return list[rd];
        }

        public static T GetRandomInArray<T>(T[] list, Random random)
        {
            int rd = random.Next(list.Length);
            return list[rd];
        }


        public static int[] GetArrayRanges(Array array)
        {
            Type type = array.GetType();
            int rank = type.GetArrayRank();
            int[] ranges = new int[rank];
            for (int i = 0; i < rank; i++)
            {
                ranges[i] = array.GetLength(i);
            }
            return ranges;
        }

        public static int[] GetArrayRankIndex(int[] ranks, int total_index)
        {
            int[] ret = new int[ranks.Length];
            if (ranks.Length == 1)
            {
                ret[0] = total_index;
                return ret;
            }
            if (ranks.Length == 2)
            {
                ret[0] = total_index / ranks[1];
                ret[1] = total_index % ranks[1];
                return ret;
            }
            if (ranks.Length == 3)
            {
                ret[0] = total_index / ranks[2] / ranks[1];
                ret[1] = total_index / ranks[2] % ranks[1];
                ret[2] = total_index % ranks[2];
                return ret;
            }
            if (ranks.Length == 4)
            {
                ret[0] = total_index / ranks[3] / ranks[2] / ranks[1];
                ret[1] = total_index / ranks[3] / ranks[2] % ranks[1];
                ret[2] = total_index / ranks[3] % ranks[2];
                ret[3] = total_index % ranks[3];
                return ret;
            }
            return GetArrayIndex(ranks, total_index);
        }

        public static int GetArrayTotalIndex(int[] ranks, params int[] indices)
        {
            int total_index = 0;
            if (ranks.Length == 1)
            {
                total_index = indices[0];
                return total_index;
            }
            if (ranks.Length == 2)
            {
                total_index += indices[0] * ranks[1];
                total_index += indices[1];
                return total_index;
            }
            if (ranks.Length == 3)
            {
                total_index += indices[0] * ranks[2] * ranks[1];
                total_index += indices[1] * ranks[2];
                total_index += indices[2];
                return total_index;
            }
            if (ranks.Length == 4)
            {
                total_index += indices[0] * ranks[3] * ranks[2] * ranks[1];
                total_index += indices[1] * ranks[3] * ranks[2];
                total_index += indices[2] * ranks[3];
                total_index += indices[3];
                return total_index;
            }
            return GetArrayIndex(ranks, indices);
        }


        private static int[] GetArrayIndex(int[] arrayStruct, int index)
        {
            int[] valueArray = new int[arrayStruct.Length];
            int[] tempArray = new int[arrayStruct.Length];

            int[] outIndex = new int[arrayStruct.Length];

            valueArray[arrayStruct.Length - 1] = 1;
            for (int i = arrayStruct.Length - 1 - 1; i >= 0; --i)
            {
                valueArray[i] = arrayStruct[i + 1] * valueArray[i + 1];
            }

            if (index < 0 || index > valueArray[0] * arrayStruct[0])
                throw new Exception(" Array Out of index " + index);

            outIndex[0] = index / valueArray[0];
            tempArray[0] = outIndex[0] * valueArray[0];

            for (int i = 1; i < arrayStruct.Length; ++i)
            {
                outIndex[i] = (index - tempArray[i - 1]) / valueArray[i];
                tempArray[i] = tempArray[i - 1] + outIndex[i] * valueArray[i];
            }

            return outIndex;
        }

        private static int GetArrayIndex(int[] arrayStruct, int[] arrayIndex)
        {
            int index = 0;

            int[] valueArray = new int[arrayStruct.Length];

            valueArray[arrayStruct.Length - 1] = 1;
            for (int i = arrayStruct.Length - 1 - 1; i >= 0; --i)
            {
                valueArray[i] = arrayStruct[i + 1] * valueArray[i + 1];
            }

            for (int i = 0; i < arrayStruct.Length; ++i)
            {
                index += valueArray[i] * arrayIndex[i];
            }

            return index;
        }

        public static T GetMinOrMax<T>(T[] array, int index)
        {
            if (array.Length > 0)
            {
                if (index < 0)
                {
                    return array[0];
                }
                if (index >= array.Length)
                {
                    return array[array.Length - 1];
                }
                return array[index];
            }
            return default(T);
        }

        public static T GetMinOrMax<T>(IList<T> array, int index)
        {
            if (array.Count > 0)
            {
                if (index < 0)
                {
                    return array[0];
                }
                if (index >= array.Count)
                {
                    return array[array.Count - 1];
                }
                return array[index];
            }
            return default(T);
        }

        #endregion

    }
}
