
namespace TradingServer.Logging 
{
    public abstract class AbstractLogger: ILogger 
    {

        protected AbstractLogger() 
        {

        }
        
        public void Debug(string module, string message) => Log(message_types.Debug, module, message);
        public void Debug(string module, Exception e) => Log(message_types.Debug, module, $"{e}");
        public void LogInformation(string module, string message) => Log(message_types.LogInformation, module, message);
        public void LogInformation(string module, Exception e) => Log(message_types.LogInformation, module, $"{e}");
        public void Warning(string module, string message) => Log(message_types.Warning, module, message);
        public void Warning(string module, Exception e) => Log(message_types.Warning, module, $"{e}");
        public void Error(string module, string message) => Log(message_types.Error, module, message);
        public void Error(string module, Exception e) => Log(message_types.Error, module, $"{e}");

        protected abstract void Log(message_types type, string module, string msg);
    }
}