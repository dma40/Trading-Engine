using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class TradingEngine: IMatchingEngine, IDisposable
    {
        public int count => orderbook.count;
        public OrderbookSpread spread()
        {
            return orderbook.spread();
        }

        public List<OrderbookEntry> getAskOrders()
        {
            return orderbook.getAskOrders();
        }

        public List<OrderbookEntry> getBidOrders()
        {
            return orderbook.getBidOrders();
        }

        public bool containsOrder(long orderID)
        {
            return orderbook.containsOrder(orderID); 
        }
    }
}