using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public interface IMatchingEngine: IOrderEntryOrderbook
    {
        Trades match(Order order);
    }
}