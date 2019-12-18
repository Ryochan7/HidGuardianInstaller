using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Targets.Wrappers;

namespace HidGuardianInstaller
{
    class LoggerHolder
    {
        private Logger logger;// = LogManager.GetCurrentClassLogger();
        public Logger Logger { get => logger; }

        public LoggerHolder()
        {
            /*var configuration = LogManager.Configuration;
            var wrapTarget = configuration.FindTargetByName<WrapperTargetBase>("logfile") as WrapperTargetBase;
            var fileTarget = wrapTarget.WrappedTarget as NLog.Targets.FileTarget;
            fileTarget.FileName = $@"{Util.exepath}\hidguardinstall_log.txt";
            fileTarget.ArchiveFileName = $@"{Util.exepath}\hidguardinstall_log_{{#}}.txt";
            LogManager.Configuration = configuration;
            LogManager.ReconfigExistingLoggers();
            */

            logger = LogManager.GetCurrentClassLogger();
        }

        public void WriteToLog(string message)
        {
            logger.Info(message);
        }
    }
}
