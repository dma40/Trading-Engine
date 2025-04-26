using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public interface IRetrievalOrderbook: IReadOnlyOrderbook
    {
        new List<OrderbookEntry> getAskOrders();
        new List<OrderbookEntry> getBidOrders();

        new Trades match(Order order)
        {
            throw new UnauthorizedAccessException("401 Unauthorized Access");
        }
    }
}