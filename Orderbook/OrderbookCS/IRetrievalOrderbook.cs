using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public interface IRetrievalOrderbook: IOrderEntryOrderbook
    {
        List<OrderbookEntry> getAskOrders();
        List<OrderbookEntry> getBidOrders();
    }
}