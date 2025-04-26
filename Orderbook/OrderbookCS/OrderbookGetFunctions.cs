using TradingServer.Orders;
using TradingServer.Instrument;

namespace TradingServer.OrderbookCS
{
    public partial class Orderbook: RetrievalOrderbook, IOrderEntryOrderbook, IDisposable
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
    }
}