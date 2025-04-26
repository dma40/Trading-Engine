using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public interface IMatchingOrderbook: IOrderEntryOrderbook
    {
        new Trades match(Order order);
    }
}