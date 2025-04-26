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
            throw new UnauthorizedAccessException("401 Unauthorized Access");
        }

        new List<OrderbookEntry> getBidOrders()
        {
            throw new UnauthorizedAccessException("401 Unauthorized Access");
        }

        new Trades match(Order order)
        {
            throw new UnauthorizedAccessException("401 Unauthorized Access");
        }
    }
}