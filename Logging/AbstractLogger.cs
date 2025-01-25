using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace TradingServer.Logging {
    abstract class AbstractLogger: ILogger {

        protected AbstractLogger() 
        {
            
        }
        public void Debug(string module, string msg) => Log(message_types.Debug, module, msg);

        public void Debug(string module, Exception e) => Log(message_types.Debug, module, $"{e}");

        public void LogInformation(string module, string msg) => Log(message_types.LogInformation, module, msg);

        public void LogInformation(string module, Exception e) => Log(message_types.LogInformation, module, $"e");

        public void Warning(string module, string msg) => Log(message_types.Warning, module, msg);

        public void Warning(string module, Exception e) => Log(message_types.Warning, module, $"{e}");

        protected abstract void Log(message_types type, string module, string msg);
    }
}