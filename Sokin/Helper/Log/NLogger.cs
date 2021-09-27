using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sokin.Helper.Log
{
    class NLogger
    {
        public void InitializeLogger(string target, string filename)
        {
            var config = new NLog.Config.LoggingConfiguration();

            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = target + "\\" + filename + "_${date:format=yyyyMMdd}.log" };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logconsole);
            config.AddRule(NLog.LogLevel.Error, NLog.LogLevel.Fatal, logconsole);

            config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, logfile);
            config.AddRule(NLog.LogLevel.Error, NLog.LogLevel.Fatal, logfile);

            NLog.LogManager.Configuration = config;
        }
    }
}
