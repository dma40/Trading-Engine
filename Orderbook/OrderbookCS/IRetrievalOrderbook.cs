using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    // Interface for limiting orderbook access: we can make it so that we can only retrieve orders,
    // nothing more
    public interface IRetrievalOrderbook: IOrderEntryOrderbook
    {
        List<OrderbookEntry> getAskOrders();
        List<OrderbookEntry> getBidOrders();
    }
}