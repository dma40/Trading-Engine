using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class Orderbook: IOrderEntryOrderbook, IDisposable
    {        
        private readonly SortedSet<Limit> _askLimits = new SortedSet<Limit>(AskLimitComparer.comparer);
        private readonly SortedSet<Limit> _bidLimits = new SortedSet<Limit>(BidLimitComparer.comparer);

        public SortedSet<Limit> getAskLimits()
        {
            return _askLimits;
        }

        public SortedSet<Limit> getBidLimits()
        {
            return _bidLimits;
        }

        public List<OrderbookEntry> getAskOrders()
        {
            List<OrderbookEntry> result = new List<OrderbookEntry>();

            foreach (Limit limit in _askLimits)
            {
                OrderbookEntry? headPointer = limit.head;

                while (headPointer != null)
                {
                    result.Add(headPointer);
                    headPointer = headPointer.next;
                }
            }

            return result;
        }

        public List<OrderbookEntry> getBidOrders()
        {
            List<OrderbookEntry> result = new List<OrderbookEntry>();

            foreach (Limit limit in _bidLimits)
            {
                OrderbookEntry? headPointer = limit.head;
                
                while (headPointer != null)
                {
                    result.Add(headPointer);
                    headPointer = headPointer.next;
                }
            }

            return result;
        }

        public OrderbookSpread spread()
        {
            long? bestAsk = null, bestBid = null;
            if (_askLimits.Any() && _askLimits.Min != null && !_askLimits.Min.isEmpty)
            {
                bestAsk = _askLimits.Min.Price;
            }

            if (_bidLimits.Any() && _bidLimits.Max != null && !_bidLimits.Max.isEmpty)
            {
                bestBid = _bidLimits.Max.Price;
            }
            
            return new OrderbookSpread(bestBid, bestAsk);
        }

        public int count => _orders.Count;
        public string SecurityName => _security.name;
        
        public bool containsOrder(long orderID)
        {
            return _orders.ContainsKey(orderID);        
        }
    
        public long askVolume => getAskOrders().Count;
        public long bidVolume => getBidOrders().Count;
    }
}