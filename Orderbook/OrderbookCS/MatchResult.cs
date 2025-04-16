using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public class MatchResult
    {
        public MatchResult() 
        {
            result = new List<OrderRecord>();
        }

        public void addTransaction(OrderRecord record)
        {
            // add a new constructor that can construct this from a OrderbookEntry object
            // in addition - queue position should stay roughly the same after something; 0 if deleted from 
            // queue
            // should also only record the impact on the orders resting in the orderbook
            result.Add(record);
        }

        public List<OrderRecord> result;
        // should be a list of order records
    }
}