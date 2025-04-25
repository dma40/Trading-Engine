using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public interface IOrderEntryOrderbook: IReadOnlyOrderbook
    {
        new void addOrder(Order order);
        new void modifyOrder(ModifyOrder modify);
        new void removeOrder(CancelOrder cancel);

        new List<OrderbookEntry> getAskOrders()
        {
            throw new UnauthorizedAccessException();
        }

        new List<OrderbookEntry> getBidOrders()
        {
            throw new UnauthorizedAccessException();
        }
    }
}