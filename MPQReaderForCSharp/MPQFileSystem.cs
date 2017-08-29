using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UFramework
{

    /// <summary>
    /// MPQ 文件头
    /// </summary>
    public class MPQHeader : IReadable
    {
        public static byte[] MPQ_HEAD = { (byte)'M', (byte)'P', (byte)'Y', (byte)'Q' };

        public byte[] head { get; set; }
        public int headerSize { get; set; }
        public int versionNum { get; set; }
        public int archiveSize { get; set; }
        public int entriesCount { get; set; }
        public int entriesOffset { get; set; }
        public int blocksOffset { get; set; }

        public void Read(IInputStream input)
        {
            this.head = input.ReadBytes(4);
            this.headerSize = input.ReadInt();
            this.versionNum = input.ReadInt();
            this.archiveSize = input.ReadInt();
            this.entriesCount = input.ReadInt();
            this.entriesOffset = input.ReadInt();
            this.blocksOffset = input.ReadInt();
        }
    }

    /// <summary>
    /// MPQ内文件信息
    /// </summary>
    public class MPQEntry : IReadable
    {

        public long hashA { get; set; }
        public long hashB { get; set; }
        public int index { get; set; }
        public int keySize { get; set; }
        public string key { get; set; }
        public int fileDate { get; set; }
        public int fileMD5Size { get; set; }
        public string fileMD5 { get; set; }
        public int fileOffset { get; set; }
        public int fileSize { get; set; }

        public MPQArchive File { get; set; }

        public void Read(IInputStream input)
        {
            this.hashA = input.ReadLong();
            this.hashB = input.ReadLong();
            this.index = input.ReadInt();
            this.keySize = input.ReadInt();
            this.key = UTF8Encoding.UTF8.GetString(input.ReadBytes(keySize), 0, keySize);
            this.fileDate = input.ReadInt();
            this.fileMD5Size = input.ReadInt();
            this.fileMD5 = UTF8Encoding.UTF8.GetString(input.ReadBytes(fileMD5Size), 0, fileMD5Size);
            this.fileOffset = input.ReadInt();
            this.fileSize = input.ReadInt();
        }

    }

    /// <summary>
    /// MPQ文件
    /// </summary>
    public class MPQArchive : IDisposable
    {

        private ULogger log = ULog.GetLogger("MPQArchive - ");

        private InputStream stream;
        private MPQHeader header;

        private List<MPQEntry> entries = new List<MPQEntry>();

        private Dictionary<int, long> encryptionTable = new Dictionary<int, long>();

        public MPQArchive(string filename)
        {
            this.stream = new InputStream(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 2, false));

            this.readHeader();
            this.readEntries();
            this.prepareEncryptTable();
        }

        /// <summary>
        /// 从MPQ中读取文件数据
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public byte[] ReadFile(string filename)
        {

            MPQEntry entry = this.getEntry(filename);
            if(entry == null)
            {
                log.Error("the file is not in mpq:" + filename);
                return null;
            }

            int offset = this.header.blocksOffset + entry.fileOffset;
            this.stream.Seek(offset);
            return this.stream.ReadBytes(entry.fileSize);
        }

        /// <summary>
        /// 获取所有Entry信息
        /// </summary>
        /// <returns></returns>
        public List<MPQEntry> GetEntries()
        {
            return new List<MPQEntry>(this.entries);

        }

        /// <summary>
        /// 将MPQ中指定的文件解压到指定的文件
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="targetFile"></param>
        public void ExtractFile(string filename, string targetFile)
        {
            byte[] data = ReadFile(filename);

            string path = Path.GetDirectoryName(targetFile);
            CUtils.CreateDir(new DirectoryInfo(path));

            using (FileStream f = new FileStream(targetFile, FileMode.Create, FileAccess.Write, FileShare.Write, 1024*2, false))
            {
                f.Write(data, 0, data.Length);
                f.Close();
            }
        }

        /// <summary>
        /// 将MPQ中所有文件解压到指定的目录
        /// </summary>
        /// <param name="dir"></param>
        public void ExtractAll(string dir)
        {
            dir = CUtils.TrimPath(dir);
            foreach(MPQEntry entry in this.entries)
            {
                string filepath = dir + "/" + entry.key;
                ExtractFile(entry.key, filepath);
            }
        }

        /// <summary>
        /// MPQ文件是否正常
        /// </summary>
        public bool isValid { get; set; }

        //读取MPQ头
        private void readHeader()
        {
            this.header = new MPQHeader();
            this.header.Read(this.stream);
            if (CUtils.ArraysEqual<byte>(this.header.head, MPQHeader.MPQ_HEAD))
            {
                this.isValid = true;
            }
            else
            {
                this.isValid = false;
                log.Error("mpq head is not matched, maybe the file is not a mpq archive");
            }
        }

        /// <summary>
        /// 读取MPQ包中所有文件信息
        /// </summary>
        private void readEntries()
        {

            int enriesCount = this.header.entriesCount;
            for(int i=0; i<enriesCount; i++)
            {
                MPQEntry entry = new MPQEntry();
                entry.Read(this.stream);
                entry.File = this;
                log.Debug("read a entry file:" + entry.key+";hashA:"+entry.hashA+";hashB:"+entry.hashB);
                this.entries.Add(entry);
            }

        }

        /// <summary>
        /// 获取指定文件名称对应的Entry
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private MPQEntry getEntry(string filename)
        {
            long hash1 = hash(filename, 1);
            long hash2 = hash(filename, 2);
            foreach(MPQEntry entry in this.entries)
            {
                if(entry.key == filename && entry.hashA == hash1 && entry.hashB == hash2)
                {
                    return entry;
                }
            }

            return null;
        }

        //字符串hash
        private long hash(string str, int type)
        {
            long seed1 = 0x7FED7FED;
            long seed2 = 0xEEEEEEEE;

            foreach (char c in str.ToUpper().ToCharArray())
            {
                int ci = (int)c;
                long value = this.encryptionTable[(type << 8) + ci];
                seed1 = (value ^ (seed1 + seed2)) & 0xFFFFFFFF;
                seed2 = ci + seed1 + seed2 + (seed2 << 5) + 3 & 0xFFFFFFFF;
            }

            return seed1;
        }

        //准备hash table
        private void prepareEncryptTable()
        {
            long seed = 0x00100001;
            for (int i=0; i<256; i++)
            {
                int index = i;
                for (int j = 0; j < 5; j++)
                {
                    seed = (seed * 125 + 3) % 0x2AAAAB;
                    long temp1 = (seed & 0xFFFF) << 0x10;
                    seed = (seed * 125 + 3) % 0x2AAAAB;
                    long temp2 = (seed & 0xFFFF);
                    this.encryptionTable[index] = (temp1 | temp2);
                    index += 0x100;
                }
            }

        }

        public void Dispose()
        {
            this.stream.Close();
            this.entries.Clear();
            this.encryptionTable.Clear();
        }
    }

    
    /// <summary>
    /// MPQ文件系统
    /// </summary>
    public class MPQFileSystem : IDisposable
    {
        private ULogger log = ULog.GetLogger("MPQFileSystem - ");

        private Dictionary<string, MPQArchive> mpqFiles = new Dictionary<string, MPQArchive>();
        private Dictionary<string, MPQEntry> mpqEntries = new Dictionary<string, MPQEntry>();

        public MPQFileSystem() { }

        private DirectoryInfo dir;

        /// <summary>
        /// 初始化，读取指定目录下的MPQ到内存中
        /// </summary>
        /// <param name="mpqDir"></param>
        /// <returns></returns>
        public bool Init(DirectoryInfo mpqDir)
        {
            this.dir = mpqDir;

            foreach(FileInfo file in mpqDir.GetFiles())
            {
                if (!file.Extension.ToLower().EndsWith("mpq"))
                {
                    continue;
                }

                if(this.loadMPQ(file.FullName) == null)
                {
                    return false;
                }
                
            }

            foreach(DirectoryInfo dir in mpqDir.GetDirectories())
            {
                if (!Init(dir))
                {
                    return false;
                }
            }


            return true;
        }

        /// <summary>
        /// 解压所有MPQ到指定目录
        /// </summary>
        public void ExtractAll(string dir)
        {
            foreach(MPQArchive archive in this.mpqFiles.Values)
            {
                archive.ExtractAll(dir);
            }
        }

        /// <summary>
        /// 从当前所有MPQ文件中读取指定文件
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public byte[] ReadData(string filename)
        {

            MPQEntry entry = null;
            if (!this.mpqEntries.TryGetValue(filename, out entry))
            {
                return null;
            }

            return entry.File.ReadFile(filename);
        }

        //读取MPQ文件信息
        private MPQArchive loadMPQ(string path)
        {
            FileInfo fileinfo = new FileInfo(Path.GetFullPath(path));
            MPQArchive mpqFile = new MPQArchive(fileinfo.FullName);

            if (!mpqFile.isValid)
            {
                log.Error("the mpq file is not valid:" + path);
                return null;
            }

            this.mpqFiles[fileinfo.FullName] = mpqFile;
            foreach(MPQEntry entry in mpqFile.GetEntries())
            {
                this.putEntry(entry);
            }

            return mpqFile;
        }


        //添加一个entry，注意新老覆盖
        private bool putEntry(MPQEntry entry)
        {
            MPQEntry old = null;
            if(!this.mpqEntries.TryGetValue(entry.key, out old))
            {
                this.mpqEntries[entry.key] = entry;
                return true;
            }

            if(old.fileDate < entry.fileDate)
            {
                this.mpqEntries[entry.key] = entry;
                return true;
            }

            return false;
        }


        public void Dispose()
        {
            foreach(MPQArchive file in this.mpqFiles.Values)
            {
                file.Dispose();
            }

            this.mpqFiles.Clear();
            this.mpqEntries.Clear();
        }
    }

}

