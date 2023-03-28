using System;
using FileManagerCLI.Enums;

namespace FileManagerCLI.Data
{
    public class LogEvent
    {
        public string Log { get; set; }
        public LogType LogType { get; set; }

        public Exception Exception { get; set; }
    }
}