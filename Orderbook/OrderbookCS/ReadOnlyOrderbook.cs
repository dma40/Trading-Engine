using TradingServer.Orders;
using TradingServer.Instrument;

namespace TradingServer.OrderbookCS
{
    public class ReadOnlyOrderbook: IReadOnlyOrderbook
    {
        private readonly Security _instrument;

        private readonly Dictionary<long, OrderbookEntry> _orders = new Dictionary<long, OrderbookEntry>();

        private readonly SortedSet<Limit> _askLimits = new SortedSet<Limit>(AskLimitComparer.comparer);
        private readonly SortedSet<Limit> _bidLimits = new SortedSet<Limit>(BidLimitComparer.comparer);

        public ReadOnlyOrderbook(Security instrument)
        {
            _instrument = instrument;
        }

        public OrderbookSpread spread()
        {
            long? bestAsk = null, bestBid = null;
            if (_askLimits.Any() && !_askLimits.Min.isEmpty)
                bestAsk = _askLimits.Min.Price;
            
            if (_bidLimits.Any() && !_bidLimits.Min.isEmpty)
                bestBid = _askLimits.Max.Price;
            
            return new OrderbookSpread(bestBid, bestAsk);
        }

        public int count => _orders.Count;

        public string SecurityName => _instrument.name;
        
        public bool containsOrder(long orderID)
        {
            return _orders.ContainsKey(orderID);        
        }
    }
}