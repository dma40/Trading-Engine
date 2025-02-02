using TradingServer.Instrument;
using TradingServer.Orders;

// Todos:
// PROrderbook (Pro-Rata Orderbook), FIFOrderbook
// Matching orderbook interface
// if possible, order types + market type

namespace TradingServer.Orderbook
{
    public class Orderbook: IRetrievalOrderbook
    {
        private readonly Security _instrument;
        // this orderbook contains only a single security, which is why this contains a single Security object
        private readonly SortedSet<Limit> _askLimits = new SortedSet<Limit>(AskLimitComparer.comparer);
        private readonly SortedSet<Limit> _bidLimits = new SortedSet<Limit>(BidLimitComparer.comparer);
        // Precondition: ensure that each order has a unique ID
        private readonly Dictionary<long, OrderbookEntry> _orders = new Dictionary<long, OrderbookEntry>();

        public Orderbook(Security instrument) 
        {
            _instrument = instrument;
        }

        public void addOrder(Order order)
        {
            var baseLimit = new Limit(order.Price);
            addOrder(order, baseLimit, order.isBuySide ? _askLimits : _bidLimits, _orders);
        }

        private static void addOrder(Order order, Limit baseLimit, SortedSet<Limit> levels, Dictionary<long, OrderbookEntry> orders)
        {
            OrderbookEntry orderbookEntry = new OrderbookEntry(order, baseLimit);
            if (levels.TryGetValue(baseLimit, out Limit limit))
            {
                
                if (limit.head == null)
                {
                    limit.head = orderbookEntry;
                    limit.tail = orderbookEntry;
                }
                else
                {
                    OrderbookEntry tailPointer = limit.tail;
                    tailPointer.next = orderbookEntry;
                    orderbookEntry.previous = tailPointer;
                    limit.tail = orderbookEntry;
                }
                orders.Add(order.OrderID, orderbookEntry);
            }
            else 
            {
                levels.Add(baseLimit);

                baseLimit.head = orderbookEntry;
                baseLimit.tail = orderbookEntry;

                orders.Add(order.OrderID, orderbookEntry);
            }
        }

        public void removeOrder(CancelOrder cancel)
        {
            // we need to find the appropriate ID, and then cancel it
            if (_orders.TryGetValue(cancel.OrderID, out OrderbookEntry orderbookentry))
            {
                removeOrder(cancel, orderbookentry, _orders);
            }
            // testcase for removing from head, removing from tail, removing from middle
        }

        public void modifyOrder(ModifyOrder modify)
        // modify from head, modify from tail, modify from middle, modify a empty 
        {
            if (_orders.TryGetValue(modify.OrderID, out OrderbookEntry orderentry))
            {
                removeOrder(modify.cancelOrder());
                addOrder(modify.newOrder(), orderentry.ParentLimit, modify.isBuySide ? _bidLimits : _askLimits, _orders);
                // find the corresponding entry
                // then remove it 
                // test if it is null
            }
        }

        public void removeOrder(CancelOrder cancel, OrderbookEntry orderentry, Dictionary<long, OrderbookEntry> orders)
        {

        }

        public int count => _orders.Count;

        public List<OrderbookEntry> getAskOrders()
        {
            List<OrderbookEntry> entries = new List<OrderbookEntry>();
            foreach (var limit in _askLimits)
            {
                var headPointer = limit.head;
                while (headPointer != null)
                {
                    entries.Add(headPointer);
                    headPointer = headPointer.next;
                }
            }
            return entries;
        }

        public List<OrderbookEntry> getBidOrders()
        {
            List<OrderbookEntry> entries = new List<OrderbookEntry>();
            foreach (var limit in _bidLimits)
            {
                var headPointer = limit.head;
                while (headPointer != null)
                {
                    entries.Add(headPointer);
                    headPointer = headPointer.next;
                }
            }
            return entries;
        }
        public bool containsOrder(long orderID)
        {
            return _orders.ContainsKey(orderID);
        }
        public OrderbookSpread spread()
        {
            long? bestAsk = null, bestBid = null;
            if (_askLimits.Any() && !_askLimits.Min.isEmpty)
            {
                bestAsk = _askLimits.Min.Price;
            }

            if (_bidLimits.Any() && !_askLimits.Min.isEmpty)
            {
                bestBid = _askLimits.Max.Price;
            }
            return new OrderbookSpread(bestBid, bestAsk);
        }
    }
}