using System.IO.Pipelines;

namespace TradingServer.OrderbookCS
{
    public enum PermissionLevel
    {
        ReadOnly,
        Retrieval,
        OrderEntry,
        Trading
    }
}