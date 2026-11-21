using AstmDriver.Abstractions;
using System;
using System.IO;
using System.Threading;

namespace AstmDriver.Core
{
    public class FileLogger : ILogger
    {
        private int _logLevel = 0;
        static ReaderWriterLock l_locker = new ReaderWriterLock();
        static ReaderWriterLock c_locker = new ReaderWriterLock();

        public FileLogger()
        {

        }

        public FileLogger(int logLevel)
        {
            _logLevel = logLevel;
        }

        public string TypeName { get; set; }

        public void Error(string error)
        {
            Write("Error", error);
        }

        public void Error(Exception ex, string error)
        {
            Write("Exception", $"{error}{Environment.NewLine}{ex.ToString()}" );
        }

        public void Trace(string traceMessage)
        {
            if(_logLevel >= 1 && _logLevel <= 3)
                Write("Trace", traceMessage);
        }

        public void Info(string message)
        {
            if (_logLevel >= 2 && _logLevel <= 3)
                Write("Info ", message);
        }
        public void Debug(string message)
        {
            if (_logLevel >= 3)
                Write("Debug", message);
        }


        private void Write(string type, string message)
        {
            var data = new string[] {
                DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss ffff") + " |  " + type + "  |  " + message,
            };

            string fileName = DateTime.Now.ToString("dd-MM-yyyy") + "_log.txt";
            string path = Path.Combine(Environment.CurrentDirectory, fileName);

            try
            {
                l_locker.AcquireWriterLock(int.MaxValue);
                File.AppendAllLines(path, data);
            }
            finally
            {
                l_locker.ReleaseWriterLock();
            }
        }

        public void WriteCrashDump(string data) 
        {
            try
            {
                c_locker.AcquireWriterLock(int.MaxValue);
                string fileName = DateTime.Now.ToString("dd-MM-yyyy") + "_crash_dump.txt";
                string path = Path.Combine(Environment.CurrentDirectory, fileName);
                File.AppendAllText(path, data);
            }
            finally
            {
                c_locker.ReleaseWriterLock();
            }
        }
    }

}
