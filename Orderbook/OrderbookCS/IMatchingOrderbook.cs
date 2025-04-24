using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public interface IMatchingOrderbook: IRetrievalOrderbook
    {
        Trades match(Order order);
    }
}