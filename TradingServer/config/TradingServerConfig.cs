namespace TradingServer.Core.Configuration 
{
    class TradingServerConfiguration 
    {
        public TradingServerSettings? TradingServerSettings { get; set; }
    }

    class TradingServerSettings 
    {
        public int Port { get; set; }
        public string? SecurityName { get; set; } 
        public string? SecurityID { get; set; }
    }
}