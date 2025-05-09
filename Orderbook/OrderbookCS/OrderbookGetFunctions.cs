using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class OrderEntryOrderbook: RetrievalOrderbook, IOrderEntryOrderbook, IDisposable
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

        public new List<OrderbookEntry> getAskOrders()
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

        public new List<OrderbookEntry> getBidOrders()
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
    
    public long askVolume => getAskOrders().Count;
    public long bidVoume => getBidOrders().Count;

    }
}