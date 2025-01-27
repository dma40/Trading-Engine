using System;

namespace TradingServer.Logging {
    public interface ILogger {
        void Debug(string module, string msg);

        void Debug(string module, Exception e);

        void LogInformation(string module, string msg);

        void LogInformation(string module, Exception e);

        void Warning(string module, string msg);

        void Warning(string module, Exception e);

        void Error(string module, string msg);

        void Error(string module, Exception e);

    }
}