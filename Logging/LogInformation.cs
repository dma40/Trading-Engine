namespace TradingServer.Logging 
{
    public sealed record LogInformation(message_types type, string module, string message, 
                                DateTime now, int id, string name);
}

