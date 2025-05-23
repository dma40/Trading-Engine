using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public interface IOrderEntryOrderbook: IRetrievalOrderbook
    {
        void addOrder(Order order);
        void modifyOrder(ModifyOrder modify);
        void removeOrder(CancelOrder cancel);
    }
}