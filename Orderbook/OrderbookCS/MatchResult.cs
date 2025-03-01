using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public class MatchResult
    {
        public MatchResult() 
        {
            result = new List<OrderRecord>();
        }

        public void addTransaction(OrderbookEntry order)
        {
            // add a new constructor that can construct this from a OrderbookEntry object
            // in addition - queue position should stay roughly the same after something; 0 if deleted from 
            // queue
            Order o = order.CurrentOrder;
            result.Add(new OrderRecord(o.OrderID, o.Quantity, o.Price, o.isBuySide, o.Username, o.SecurityID, order.queuePosition()));
        }

        public List<OrderRecord> result;
        // should be a list of order records
    }
}