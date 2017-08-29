using System;
using System.Collections.Generic;

namespace UFramework
{
    /// <summary>
    /// 日志输出接口
    /// </summary>
    public interface ILogWriter
    {
        void Write(string msg);
    }

    /// <summary>
    /// 统一日志接口
    /// </summary>
    public class ULogger
    {
        public const int LEVEL_TRACE = 0;
        public const int LEVEL_DEBUG = 1;
        public const int LEVEL_INFO = 2;
        public const int LEVEL_WARN = 3;
        public const int LEVEL_ERROR = 4;

        protected string name = "ulog - ";
        protected int currLevel = LEVEL_DEBUG;

        private ILogWriter printer;

        public ULogger() { }

        public ULogger(string name)
        {
            this.name = name;
        }

        public ULogger(string name, int level, ILogWriter printer) : this(name)
        {
            this.currLevel = level;
            this.printer = printer;
        }

        public void Log(int level, string msg)
        {
            if (this.currLevel <= level && printer != null)
            {
                printer.Write(name + msg);
            }
        }

        public void SetName(string name)
        {
            this.name = name;
        }

        public void SetLevel(int level)
        {
            this.currLevel = level;
        }

        public void SetWriter(ILogWriter printer)
        {
            this.printer = printer;
        }

        public void Trace(string msg)
        {
            Log(LEVEL_TRACE, "TRACE::" + msg);
        }

        public void Debug(string msg)
        {
            Log(LEVEL_DEBUG, "DEBUG::" + msg);
        }

        public void Info(string msg)
        {
            Log(LEVEL_INFO, "INFO::" + msg);
        }

        public void Warn(string msg)
        {
            Log(LEVEL_WARN, "WARN::" + msg);
        }

        public void Warn(string msg, System.Exception e)
        {
            Log(LEVEL_WARN, "WARN::" + msg + "\r\n" + e.Message + "\r\n" + e.StackTrace);
        }

        public void Error(string msg)
        {
            Log(LEVEL_ERROR, "ERROR::" + msg);
        }

        public void Error(System.Exception e)
        {
            Log(LEVEL_ERROR, "ERROR::" + e.Message + "\r\n" + e.StackTrace);
        }

        public void Error(string msg, System.Exception e)
        {
            Log(LEVEL_ERROR, "ERROR::" + msg + "\r\n" + e.Message + "\r\n" + e.StackTrace);
        }

    }

    /// <summary>
    /// 控制台输出
    /// </summary>
    public class ConsoleLogWriter : ILogWriter
    {
        public void Write(string msg)
        {
            Console.WriteLine(msg);
        }
    }

    /// <summary>
    /// 组合多种方式日志输出
    /// </summary>
    public class MultiLogWriter : ILogWriter
    {
        protected List<ILogWriter> printers = new List<ILogWriter>();

        public MultiLogWriter AddWriter(ILogWriter printer)
        {
            if(!printers.Contains(printer))
                printers.Add(printer);

            return this;
        }

        public void Write(string msg)
        {
            foreach(ILogWriter printer in printers)
            {
                printer.Write(msg);
            }
        }
    }

    /// <summary>
    /// 日志类记录和获取
    /// </summary>
    public static class ULog
    {

        private static Dictionary<string, ULogger> loggers = new Dictionary<string, ULogger>();

        private static ILogWriter logWriter;

        public static ULogger GetLogger(string name, int level = ULogger.LEVEL_DEBUG)
        {

            lock (loggers)
            {
                ULogger logger = null;
                if (!loggers.TryGetValue(name, out logger))
                {
                    logger = new ULogger(name);
                    logger.SetLevel(level);
                    logger.SetWriter(logWriter);
                    loggers.Add(name, logger);
                }

                return logger;
            }

        }

        public static void SetLogWriter(ILogWriter writer)
        {
            logWriter = writer;
        }
    }
}
