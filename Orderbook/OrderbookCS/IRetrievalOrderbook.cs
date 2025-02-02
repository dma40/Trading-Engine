using TradingServer.Orders;

namespace TradingServer.Orderbook
{
    public interface IRetrievalOrderbook: IOrderEntryOrderbook
    {
        List<OrderbookEntry> getAskOrders();
        List<OrderbookEntry> getBidOrders();
    }
}