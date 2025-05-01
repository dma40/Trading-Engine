using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public interface IOrderEntryOrderbook: IRetrievalOrderbook
    {
        new void addOrder(Order order);
        new void modifyOrder(ModifyOrder modify);
        new void removeOrder(CancelOrder cancel);
    }
}