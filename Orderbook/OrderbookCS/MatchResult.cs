using TradingServer.Orders;

namespace TradingServer.Orderbook
{
    public class MatchResult
    {
        public MatchResult() 
        {
            result = new List<OrderRecord>();
        }

        public void addTransaction(long orderID, uint quantity, long price, 
                                    bool isBuySide, string username, int SecurityID)
        {
            // add a new constructor that can construct this from a OrderbookEntry object
            // in addition - queue position should stay roughly the same after something; 0 if deleted from 
            // queue
            result.Add(new OrderRecord(orderID, quantity, price, isBuySide, username, SecurityID, 0));
        }

        public List<OrderRecord> result;
        // should be a list of order records
    }
}