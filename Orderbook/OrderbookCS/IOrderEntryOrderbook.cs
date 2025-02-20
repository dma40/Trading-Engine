using TradingServer.Orders;

namespace TradingServer.Orderbook
{
    // This adds a level of permission in that we can change the orderbook, 
    // adding a level of permission from the IReadOnlyOrderbook
    public interface IOrderEntryOrderbook: IReadOnlyOrderbook
    {
        void addOrder(Order order);
        void modifyOrder(ModifyOrder modify);
        void removeOrder(CancelOrder cancel);
    }
}