using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public class MatchResult
    {
        public MatchResult() 
        {
            result = new List<OrderRecord>();
        }

        public void addTransaction(OrderbookEntry order, uint _finalPrice, uint _initialQuantity, uint _finalQuantity, uint _queuePositionInitial)
        {
            // add a new constructor that can construct this from a OrderbookEntry object
            // in addition - queue position should stay roughly the same after something; 0 if deleted from 
            // queue
            // should also only record the impact on the orders resting in the orderbook
            Order o = order.CurrentOrder;
            result.Add(new OrderRecord(o.OrderID, _initialQuantity, _finalQuantity, 
            o.Price, _finalPrice, o.isBuySide, 
            o.Username, o.SecurityID, 
            _queuePositionInitial, order.queuePosition()));
        }

        public List<OrderRecord> result;
        // should be a list of order records
    }
}