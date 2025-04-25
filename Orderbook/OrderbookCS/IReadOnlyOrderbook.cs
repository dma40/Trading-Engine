using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public interface IReadOnlyOrderbook 
    {
        bool containsOrder(long orderID);
        int count { get; }
        OrderbookSpread spread();

        public void addOrder(Order order)
        {
            throw new UnauthorizedAccessException();
        }

        public void modifyOrder(ModifyOrder modify)
        {
            throw new UnauthorizedAccessException();
        }

        public void removeOrder(CancelOrder cancel)
        {
            throw new UnauthorizedAccessException();
        }

        public List<OrderbookEntry> getBidOrders()
        {
            throw new UnauthorizedAccessException();
        }

        public List<OrderbookEntry> getAskOrders()
        {
            throw new UnauthorizedAccessException();
        }

        Trades match(Order order)
        {
            throw new UnauthorizedAccessException();
        }
    }
}
