using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public interface IRetrievalOrderbook: IOrderEntryOrderbook
    {
        new List<OrderbookEntry> getAskOrders();
        new List<OrderbookEntry> getBidOrders();

        new Trades match(Order order)
        {
            throw new InvalidOperationException();
        }
    }
}