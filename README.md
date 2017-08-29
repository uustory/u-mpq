# u-mpq

类似暴雪MPQ归档资源打包和读取工具

**MPQ文件简介**

在游戏制作中，会有很多资源，比如图片，json,txt,xml等各种游戏资源。如果直接将这些文件放在游戏包的目录下， 会造成碎文件过多，影响文件读取速度。
同时，很多游戏都有热更新， 需要对游戏资源做增量更新。比如初始包体是100M，后面更新了部分资源，需要在已有的包体运行初始化的时候，下载这部分改动或者添加的资源。

之前暴雪针对这些需求，实现了一个优化了的资源归档格式——MPQ。 该工具在其理论基础上， 针对手游做了一些调整和简化，但是原理一致。

这个工具的作用可以简单地概括为两句话：

1、将指定目录下的所有变更或者新增的资源，打包生成一个新的mpq文件。
2、程序运行时， 从mpq中读取资源时，自动采用较新的资源。


**目录结构**
mpqpacker：

    ----mpq.py MPQ打包工具脚本，需要安装python 2.*版本。建议安装python 2.7.10以上版本

MPQReaderForCSharp：

    ----MPQFileSystem.cs：MPQ文件格式，C#解析类， 使用该类进行mpq文件的读取
    ----其他辅助类


**使用说明**

1、打包方式：

~~~~~~
python mpq.py [MPQ target dir] [Res source dir]

比如：

python mpq.py ./GameUpdates ./GameResources

就是将GameResoures下面所有的资源， 生成一个mpq文件，到GameUpdates目录下

~~~~~~

2、解压方式：

如果要解压已经存在的mpq文件，则使用如下命令

~~~~~~
python mpq.py -d [MPQ dir] [Res target dir]   -- 将[MPQ dir]目录下的mpq文件，解压到[Res target dir]目录下

比如：

python mpq.py -d ./GameUpdates ./GameResources

就是将GameUpdates目录下所有的mpq文件中的资源文件， 解压生成到GameReosurces目录下。


~~~~~~

3、程序中使用方式(C#)：

~~~~~~
初始化，指定mpq所在目录：
        string path = "C:/game/res";        //游戏中mpq资源所在目录
        MPQFileSystem mpqFileSystem = new MPQFileSystem();
        mpqFileSystem.Init(new DirectoryInfo(path));

从mpq文件中读取指定的资源：
        string path = "/assets/res/test.xml";   //在mpq中的文件，相对路径
        byte[] ret = this.mpqFileSystem.ReadData(path);

这样就从MPQ文件中读取出了指定资源文件的二进制数据。

~~~~~~

4、实际使用建议：

4.1、mpq作为资源增量打包和读取方式

4.2、对生成的mpq文件进行gzip压缩

4.3、对mpq文件生成文件索引文件， 做资源更新和检查。 游戏中初始化的时候， 先判断资源服务器上是否有新资源mpq文件， 有的话就下载， 然后程序解压并读取。

