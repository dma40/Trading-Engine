namespace TradingServer.Logging 
{
    public record LogInformation(message_types type, string module, string message, 
                                DateTime now, int id, string name);
}

