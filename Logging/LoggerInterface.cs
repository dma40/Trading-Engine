namespace TradingServer.Logging 
{
    public interface ILogger 
    {
        void Debug(string module, string message);
        void Debug(string module, Exception exception);
        
        void LogInformation(string module, string message);
        void LogInformation(string module, Exception exception);

        void Warning(string module, string message);
        void Warning(string module, Exception exception);

        void Error(string module, string message);
        void Error(string module, Exception exception);
    }
}