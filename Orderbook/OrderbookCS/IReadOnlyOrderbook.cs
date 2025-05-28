using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public interface IReadOnlyOrderbook
    {
        bool containsOrder(long orderID);
        int count { get; }
        OrderbookSpread spread();
    }
}
