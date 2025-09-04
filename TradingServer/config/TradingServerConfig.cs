using TradingServer.OrderbookCS;

namespace TradingServer.Core.Configuration 
{
    internal sealed class TradingServerConfiguration 
    {
        public TradingServerSettings? TradingServerSettings { get; set; }
    }

    internal sealed class TradingServerSettings 
    {
        public int? Port { get; set; }
        public string? SecurityName { get; set; } 
        public string? SecurityID { get; set; }
    }
}