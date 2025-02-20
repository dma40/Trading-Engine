
using TradingServer.Instrument;
using TradingServer.Orders;

// Todos:
// PROrderbook (Pro-Rata Orderbook), FIFOrderbook
// Matching orderbook interface
// if possible, order types + market type

// Also, FIFO Orderbooks consider orders based on the time at which they come in
// the usefulness of why we instantiate the UTC time to find a way of determining the earliest
// orders

namespace TradingServer.Orderbook
{
    public class Orderbook: IRetrievalOrderbook
    {
        // In this orderbook, we define it so that 
        // orders limits are unique - when we add a order to a price level that 
        // already exists, we add it to the corresponding limit.
        // Otherwise, we will create a new Limit with a null head and null tail and add it to that limit level.

        // Instance attributes
        private readonly Security _instrument;
        public readonly SortedSet<Limit> _askLimits = new SortedSet<Limit>(AskLimitComparer.comparer);
        public readonly SortedSet<Limit> _bidLimits = new SortedSet<Limit>(BidLimitComparer.comparer);
        public readonly Dictionary<long, OrderbookEntry> _orders = new Dictionary<long, OrderbookEntry>();

        // Constructor
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
            }
            else 
            {
                levels.Add(baseLimit);

                baseLimit.head = orderbookEntry;
                baseLimit.tail = orderbookEntry;
            }

            orders.Add(order.OrderID, orderbookEntry);
        }

        public void removeOrder(CancelOrder cancel)
        {
            if (_orders.TryGetValue(cancel.OrderID, out OrderbookEntry orderbookentry))
            {
                removeOrder(cancel.OrderID, orderbookentry, _orders);
            }
        }

        public void removeOrder(long id, OrderbookEntry orderentry, Dictionary<long, OrderbookEntry> orders)
        {
            if (orderentry.previous != null && orderentry.next != null)
            {
                orderentry.next.previous = orderentry.previous;
                orderentry.previous.next = orderentry.next;
            }

            else if (orderentry.previous != null)
            {
                orderentry.previous.next = null;
            }

            else if (orderentry.next != null)
            {
                orderentry.next.previous = null;
            }

            if (orderentry.ParentLimit.head == orderentry && orderentry.ParentLimit.tail == orderentry)
            {
                orderentry.ParentLimit.head = null;
                orderentry.ParentLimit.tail = null;
            }

            else if (orderentry.ParentLimit.head == orderentry && orderentry.ParentLimit.tail != orderentry)
            {
                orderentry.ParentLimit.head = orderentry.next;
            }

            else if (orderentry.ParentLimit.tail == orderentry)
            {
                orderentry.ParentLimit.tail = orderentry.previous;
            }

            _orders.Remove(id);
        }

        public void modifyOrder(ModifyOrder modify)
        // modify from head, modify from tail, modify from middle, modify a empty 
        {
            if (_orders.TryGetValue(modify.OrderID, out OrderbookEntry orderentry))
            {
                removeOrder(modify.cancelOrder());
                addOrder(modify.newOrder(), orderentry.ParentLimit, modify.isBuySide ? _bidLimits : _askLimits, _orders);
            }
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
            if (_askLimits.Any() && ! _askLimits.Min.isEmpty)
            {
                bestAsk = _askLimits.Min.Price;
            }

            if (_bidLimits.Any() && ! _askLimits.Min.isEmpty)
            {
                bestBid = _askLimits.Max.Price;
            }
            return new OrderbookSpread(bestBid, bestAsk);
        }

        public SortedSet<Limit> getAskLimits()
        {
            return _askLimits;
        }

        public SortedSet<Limit> getBidLimits()
        {
            return _bidLimits;
        }

        public bool canMatch()
        {
        // Determines if a match can happen in this limit orderbook
            foreach (var ask in _askLimits)
            {
                foreach (var bid in _bidLimits)
                {
                    if (ask.Price < bid.Price)
                        return true;
                }
            }
            return false;
        }
    }
}