namespace TradingServer.Logging 
{
    public abstract class AbstractLogger: ILogger 
    {
        protected AbstractLogger()
        {

        }
        
        public void Debug(string module, string message) => Log(message_types.Debug, module, message);
        public void Debug(string module, Exception exception) => Log(message_types.Debug, module, $"{exception.Message}");

        public void LogInformation(string module, string message) => Log(message_types.LogInformation, module, message);
        public void LogInformation(string module, Exception exception) => Log(message_types.LogInformation, module, $"{exception.Message}");

        public void Warning(string module, string message) => Log(message_types.Warning, module, message);
        public void Warning(string module, Exception exception) => Log(message_types.Warning, module, $"{exception.Message}");

        public void Error(string module, string message) => Log(message_types.Error, module, message);
        public void Error(string module, Exception exception) => Log(message_types.Error, module, $"{exception.Message}");

        protected abstract void Log(message_types type, string module, string message);
    }
}