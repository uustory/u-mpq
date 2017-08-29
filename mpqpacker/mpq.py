#!/usr/bin/env python
# -*- coding: utf-8 -*-
#Author: chenjie

import os
import os.path
import zipfile
import re
import subprocess
import platform
import sys
import shutil
import hashlib
import struct
import io
import time
import argparse
from collections import namedtuple


MPQ_HEAD = 'MPYQ'
VERSION_NUM = 1

#MPQ头
MPQHeader = namedtuple("MPQHeader",
    '''
    archive_head
    header_size
    version_num
    archive_size
    entries_count
    entries_offset
    blocks_offset
    '''
    )

MPQHeader.struct_format = '<4s6I'       #little-endian, 4byte archive_head + 6 unsigned int


#MPQ文件信息
class MPQEntry(object):

    def __init__(self):
        pass


    def write(self, fout):
        """
            fout:是一个文件对象，将当前MPQEntry序列化到fout中
        """

        fout.write(struct.pack('<Q', self.hash_a))
        fout.write(struct.pack('<Q', self.hash_b))
        fout.write(struct.pack('<I', self.curr_index))
        fout.write(struct.pack('<I', self.name_size))
        fout.write(struct.pack('<%ds' % self.name_size, self.file_name))
        fout.write(struct.pack('<I', self.file_date))
        fout.write(struct.pack('<I', self.file_md5_size))
        fout.write(struct.pack('<%ds' % self.file_md5_size, self.file_md5))
        fout.write(struct.pack('<I', self.file_offset))
        fout.write(struct.pack('<I', self.file_size))


    def read(self, fin):

        """
            fin:是一个文件对象，从fin中反序列化一个完整的MPQEntry对象
            struct.unpack返回的是一个tuple，哪怕只有一个值，所以采用arg,=struct.unpack()方式
        """

        data = fin.read(8)
        self.hash_a, = struct.unpack('<Q', data)
        data = fin.read(8)
        self.hash_b, = struct.unpack('<Q', data)
        data = fin.read(4)
        self.curr_index, = struct.unpack('<I', data)
        data = fin.read(4)
        self.name_size, = struct.unpack('<I', data)
        data = fin.read(self.name_size)
        self.file_name, = struct.unpack('<%ds' % self.name_size, data)
        data = fin.read(4)
        self.file_date = struct.unpack('<I', data)
        data = fin.read(4)
        self.file_md5_size, = struct.unpack('<I', data)
        data = fin.read(self.file_md5_size)
        self.file_md5, = struct.unpack('<%ds' % self.file_md5_size, data)        
        data = fin.read(4)
        self.file_offset, = struct.unpack('<I', data)
        data = fin.read(4)
        self.file_size, = struct.unpack('<I', data)


    def size(self):

        return 40 + self.name_size + self.file_md5_size



class MPQArchive(object):
    """
        MPQ归档文件， 可以将多个资源文件打包成为一个.mpq文件
        MPQArchive有两种模式，可读和可写。创建MPQArchive的时候，就指定了当前是可读还是可写模式，创建之后无法修改。
        可读模式，可以读取一个指定的.mpq文件，可以对mpq中的文件进行读取或者解压
        可写模式，可以创建一个指定的.mpq文件，可以将指定目录下的文件添加到mpq中
    """

    def __init__(self, resRoot, mpqFile, mode):

        """
            mode为当前文件的读写方式，可以选择的方式是【rb】和【wb】,【rb】为可读方式；【wb】为可写方式
            可读方式时：resRoot为资源根目录，作为解压时的目标目录，mpqFile为已经存在的.mpq文件
            可写方式时：resRoot为资源根目录，mpqFile是将要创建的.mpq文件的名称
        """

        self.pathRoot = self._formatpath(resRoot)
        if self.pathRoot.endswith("/"):
            self.pathRoot = self.pathRoot[0:-1]

        self.head_size = 28

        self.currIndex = 0
        self.currOffset = 0
        self.entries = list()
        self.mode = mode
        
        self.file = None
        self.mpqFile = mpqFile
        if self._readable():
            self.file = open(mpqFile, mode)
            self.read_header()
            self.read_entries()


    def read_header(self):

        """
            读取MPQ头
        """

        if self._writable():
            print("curr mode is write, cannot read")
            return

        data = self.file.read(self.head_size)
        self.header = MPQHeader._make(struct.unpack(MPQHeader.struct_format, data))
        # print(self.header)


    def write_header(self, archiveSize, entriesCount, entriesOffset, blocksOffset):

        """
            写入MPQ头
        """

        if self._readable():
            print("curr mode is read, cannot write header")
            return

        self.file.write(struct.pack(MPQHeader.struct_format, 
                                    MPQ_HEAD, 
                                    self.head_size, 
                                    VERSION_NUM,
                                    archiveSize,
                                    entriesCount,
                                    entriesOffset,
                                    blocksOffset))



    def read_entries(self):

        """
            读取MPQ所有的文件的entry信息
        """

        if self._writable():
            print("curr mode is write, cannot read")
            return

        entriesCount = self.header.entries_count
        if entriesCount <= 0:
            print("invalid entries count :"+entriesCount)
            return

        self.file.seek(self.header.entries_offset)
        for i in range(0, entriesCount):
            entry = MPQEntry()
            entry.read(self.file)

            readableEntry = dict()
            readableEntry['fullname'] = self._fullname(entry.file_name)
            readableEntry['filename'] = entry.file_name
            readableEntry['entry'] = entry

            self.entries.append(readableEntry)


    def read_file(self, filename):

        """
            从MPQ中读取指定的文件
        """

        if self._writable():
            print("curr mode is write, cannot read")
            return None

        entry = self._get_entry(filename)

        if not entry:
            print("the file is not in curr mpq:"+filename)
            return None

        offset = self.header.blocks_offset + entry.file_offset
        self.file.seek(offset)
        data = self.file.read(entry.file_size)

        if not data:
            print("file read error:"+filename)
            return None

        return data


    def extract_file(self, filename):

        """
            从MPQ中解压出指定的文件，解压目标位置是创建时指定的resRoot
        """

        print("extract_file:"+filename)

        data = self.read_file(filename)
        if not data:
            return

        targetFile = self._fullname(filename)

        self._makepath(targetFile)

        with io.open(targetFile, 'wb') as f:
            f.write(data)


    def extract_all(self):
        """
            从MPQ中解压出所有文件，解压目标位置是创建时指定的resRoot
        """

        for we in self.entries:
            self.extract_file(we['filename'])


    def gen_entry(self, filename):

        """
            生成一个entry
        """

        if self._readable():
            print("curr mode is read, cannot add file")
            return

        fullname = self._fullname(filename)

        if not os.path.exists(fullname):
            print("the file is not exists:"+fullname)

        self.currIndex += 1
        filesize = os.path.getsize(fullname)

        entry = MPQEntry()

        entry.hash_a = self._hash(filename, 'HASH_A')
        entry.hash_b = self._hash(filename, 'HASH_B')
        entry.curr_index = self.currIndex
        entry.name_size = len(bytearray(filename, 'utf_8'))
        entry.file_name = filename
        entry.file_date = int(time.time())
        entry.file_md5 = self._file_md5(fullname)
        entry.file_md5_size = len(bytearray(entry.file_md5, 'utf_8'))

        entry.file_offset = self.currOffset
        entry.file_size = filesize

        return entry


    def add_entry(self, entry):

        self.currOffset += entry.file_size

        writableEntry = dict()
        writableEntry['fullname'] = self._fullname(entry.file_name)
        writableEntry['filename'] = entry.file_name
        writableEntry['entry'] = entry

        self.entries.append(writableEntry)


    def flush(self):

        """
            将添加的所有的文件，写入到指定的MPQ文件中。add_file的时候，仅仅添加的entry信息，并没有写入到mpq文件中。只有
            flush的时候，才会将文件写入到最终的.mpq文件中
        """

        if not self._writable():
            return

        if len(self.entries) <= 0:
            print("there is no file in mpq, don't need to flush")
            return


        self.file = open(self.mpqFile, self.mode)

        self.write_header(self._archive_size(), len(self.entries), self._entries_offset(), self._blocks_offset())

        for we in self.entries:
            entry = we['entry']
            entry.write(self.file)


        for we in self.entries:
            print("flush : "+we['filename'], "hash_a:"+str(we['entry'].hash_a)+";hash_b:"+str(we['entry'].hash_b))
            with io.open(we['fullname'], 'rb') as f:
                data = f.read()
                if not data:
                    print("mpq flush error. the file no data or error:"+we['fullname'])
                    break

                self.file.write(data)

        print("created a new mpq file:"+self.mpqFile)


    def get_entries(self):
        result = list()
        for e in self.entries:
            result.append(e['entry'])

        return result


    def close(self):
        if self._writable():
            self.flush()

        if self.file:
            self.file.close()

        self.entries = None


    #当前是否为可读状态
    def _readable(self):

        return 'r' in self.mode

    #当前是否为可写状态
    def _writable(self):

        return 'w' in self.mode

    #获取指定文件的完整路径
    def _fullname(self, filename):
        return self._formatpath(os.path.join(self.pathRoot, filename))


    #循环创建文件所在的目录
    def _makepath(self, filename):

        filepath = os.path.dirname(filename)
        if not os.path.exists(filepath):
            os.makedirs(filepath)


    #格式化文件路径
    def _formatpath(self, filename):
        return filename.replace("\\", "/")


    #计算指定字符串的hash值
    def _hash(self, str, hash_type):

        """Hash a string using MPQ's hash function."""
        hash_types = {
            'HASH_A': 1,
            'HASH_B': 2
        }
        seed1 = 0x7FED7FED
        seed2 = 0xEEEEEEEE

        for ch in str.upper():
            if not isinstance(ch, int): ch = ord(ch)
            value = self.encryption_table[(hash_types[hash_type] << 8) + ch]
            seed1 = (value ^ (seed1 + seed2)) & 0xFFFFFFFF
            seed2 = ch + seed1 + seed2 + (seed2 << 5) + 3 & 0xFFFFFFFF

        return seed1


    #根据文件名获取文件对应的entry
    def _get_entry(self, filename):

        hash_a = self._hash(filename, 'HASH_A')
        hash_b = self._hash(filename, 'HASH_B')
        for we in self.entries:
            entry = we['entry']
            if entry.hash_a == hash_a and entry.hash_b == hash_b:
                return entry


    #计算MPQ文件的大小
    def _archive_size(self):
        size = self.head_size
        size += self._entries_size()

        for we in self.entries:
            size += we['entry'].file_size

        return size


    #计算所有文件entry信息的大小
    def _entries_size(self):
        size = 0
        for we in self.entries:
            size += we['entry'].size()

        return size


    #计算文件数据存储偏移位置
    def _blocks_offset(self):
        return self.head_size + self._entries_size()


    #计算entry数据存储偏移位置
    def _entries_offset(self):

        return self.head_size


    #计算文件md5
    def _file_md5(self, filepath):

        with open(filepath, 'rb') as f:
            hashobj = hashlib.md5()
            while True:
                data = f.read()
                if not data:
                    break

                hashobj.update(data)

            return hashobj.hexdigest()


    #hash seed准备
    def _prepare_encryption_table():
        """Prepare encryption table for MPQ hash function."""
        seed = 0x00100001
        crypt_table = {}

        for i in range(256):
            index = i
            for j in range(5):
                seed = (seed * 125 + 3) % 0x2AAAAB
                temp1 = (seed & 0xFFFF) << 0x10

                seed = (seed * 125 + 3) % 0x2AAAAB
                temp2 = (seed & 0xFFFF)

                crypt_table[index] = (temp1 | temp2)

                index += 0x100

        return crypt_table

    encryption_table = _prepare_encryption_table()        



class MPQFileSystem(object):

    """ MPQ 文件系统以及相关操作 """

    def __init__(self):
        pass


    def list_entries(self, mpqDir, mpqExt):

        """
            在指定的mpqDir目录下读取所有以mpqExt为后缀的文件， 获取所有的entry信息
        """

        old_entries = dict()

        if not os.path.exists(mpqDir):
            print("the mpqDir is not exists:"+mpqDir)
            return

        for root, dirs, files in os.walk(mpqDir):

            for fname in files:

                if not fname.endswith(mpqExt):
                    continue

                fullpath = os.path.join(root, fname)

                archive = MPQArchive(mpqDir, fullpath, 'rb')
                entries = archive.get_entries()

                for entry in entries:
                    old = None
                    if old_entries.has_key(entry.file_name):
                        old = old_entries[entry.file_name]

                    if old:
                        if entry.file_date > old.file_date:
                            old_entries[entry.file_name] = entry
                            print("update new file in mpq:"+entry.file_name)
                        else:
                            print("ignore old file in mpq:"+entry.file_name)
                    else:
                        old_entries[entry.file_name] = entry
                
                archive.close()      

        return old_entries


    def unpack_mpqs(self, mpqDir, targetDir, mpqExt):

        """
            将mpqDir目录下的所有mpqExt结尾的文件解压到targetDir目录
        """

        for root, dirs, files in os.walk(mpqDir):

            for fname in files:

                if not fname.endswith(mpqExt):
                    continue

                fullpath = os.path.join(root, fname)
                archive = MPQArchive(targetDir, fullpath, 'rb')
                archive.extract_all()
                archive.close()



    def generate_update(self, resDir, ignores, mpqFile, oldMpqDir, mpqExt):
        """
            从resDir目录中将所有变化了的以及新增的文件生成一个新的MPQ文件
        """

        old_entries = self.list_entries(oldMpqDir, mpqExt)

        archive = MPQArchive(resDir, mpqFile, 'wb')

        for root, dirs, files in os.walk(resDir):

            for fname in files:

                if self.is_ignore(root, fname, ignores):
                    continue

                fullpath = os.path.join(root, fname)
                if root == resDir:
                    filepath = fname
                else:
                    filepath = os.path.relpath(fullpath, resDir).replace("\\", "/")

                newEntry = archive.gen_entry(filepath)

                if not old_entries.has_key(filepath):
                    archive.add_entry(newEntry)
                    print("add new entry:"+filepath)
                elif old_entries[filepath].file_md5 != newEntry.file_md5:
                    archive.add_entry(newEntry)
                    print("update old entry:"+filepath)


        archive.close()
        
        print("all done!!!")


    #is file ignored
    def is_ignore(self, path, fname, ignores):

        if not ignores:
            return False

        for ig in ignores:
            if ig in path or fname == ig or fname.endswith(ig):
                return True

        return False


        
if __name__ == "__main__":

    parser = argparse.ArgumentParser(u"MPQ打包解包工具")
    parser.add_argument('mpqDir', help=u"MPQ文件所在的目录")
    parser.add_argument('resDir', help=u"资源文件所在的目录")
    parser.add_argument('-d', '--decompress', help=u"默认是将resDir中生成mpq文件到mpqDir中；指定-d，说明是将mpqDir中文件，解压到resDir中", action="store_true" ,default=False)

    args = parser.parse_args()

    mpqDir = args.mpqDir
    resDir = args.resDir

    if not mpqDir or not resDir:

        print("mpq dir and res dir must specified ")
        exit(-1)

    mpq = MPQFileSystem()
    if args.decompress:
        mpq.unpack_mpqs(mpqDir, resDir, ".mpq")
    else:
        filename = "res_" + str(int(time.time()))+".mpq";
        filename = os.path.join(mpqDir, filename)
        mpq.generate_update(resDir, [".meta", ".DS_Store"], filename, mpqDir, ".mpq")




