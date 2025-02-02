using TradingServer.Orders;

namespace TradingServer.Orderbook
{
    public interface IOrderEntryOrderbook: IReadOnlyOrderbook
    {
        void addOrder(Order order);
        void modifyOrder(ModifyOrder modify);
        void removeOrder(CancelOrder cancel);
    }
}