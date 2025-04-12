using System;

namespace TradingServer.Logging 
{
    public interface ILogger 
    {
        void Debug(string module, string message);
        void Debug(string module, Exception e);
        
        void LogInformation(string module, string message);
        void LogInformation(string module, Exception e);

        void Warning(string module, string message);
        void Warning(string module, Exception e);

        void Error(string module, string message);
        void Error(string module, Exception e);

    }
}