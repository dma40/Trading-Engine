using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public interface IRetrievalOrderbook: IReadOnlyOrderbook
    {
        List<OrderbookEntry> getAskOrders();
        List<OrderbookEntry> getBidOrders();
    }
}