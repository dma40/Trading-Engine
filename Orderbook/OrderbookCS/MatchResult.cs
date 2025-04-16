using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public class MatchResult
    {
        public MatchResult() 
        {
            result = new List<Trade>();
        }

        public void addTransaction(Trade trade)
        {
            // add a new constructor that can construct this from a OrderbookEntry object
            // in addition - queue position should stay roughly the same after something; 0 if deleted from 
            // queue
            // should also only record the impact on the orders resting in the orderbook
            result.Add(trade);
        }

        public List<Trade> result;
        // should be a list of order records
    }
}