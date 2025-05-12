using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public interface IMatchingEngine: IOrderEntryOrderbook
    {
        new Trades match(Order order);
    }
}