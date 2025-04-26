using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public interface ITradingOrderbook: IOrderEntryOrderbook
    {
        new Trades match(Order order);
    }
}