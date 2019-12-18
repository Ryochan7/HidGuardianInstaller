using System;
using System.IO;

namespace HidGuardianInstaller
{
    class AppLogger
    {
        public delegate void LogEventHandler(string args);
        public static event LogEventHandler LogEvent;
        private static LoggerHolder loggerHolder = new LoggerHolder();

        internal static LoggerHolder LoggerHolder { get => loggerHolder; }

        //public static StreamWriter logfile = new StreamWriter(Util.exepath + "\\log.txt", false);

        public static void Log(string message, bool toFile = true)
        {
            LogEvent?.Invoke(message);
            if (toFile)
            {
                loggerHolder.WriteToLog(message.Replace("\n", Environment.NewLine));
                //logfile.WriteLine($"{DateTime.Now.ToString()}: {message.Replace("\n", Environment.NewLine)}");
                //logfile.Flush();
            }
        }

        /*public static void CloseLogFile()
        {
            //logfile.Close();
        }
        */
    }
}
