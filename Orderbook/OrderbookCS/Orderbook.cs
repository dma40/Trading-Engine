
using TradingServer.Instrument;
using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    // wacky things seem to be happening with removing, modifying orders?
    public class Orderbook: IRetrievalOrderbook
    {
        private readonly Security _instrument;
        
        private readonly SortedSet<Limit> _askLimits = new SortedSet<Limit>(AskLimitComparer.comparer);
        private readonly SortedSet<Limit> _bidLimits = new SortedSet<Limit>(BidLimitComparer.comparer);

        private readonly Dictionary<long, OrderbookEntry> _orders = new Dictionary<long, OrderbookEntry>(); // maybe have seperate AskOrders, bidOrders to make this a little easier
        private readonly Dictionary<long, OrderbookEntry> _goodForDay = new Dictionary<long, OrderbookEntry>();
        private readonly Dictionary<long, OrderbookEntry> _fillOrKill = new Dictionary<long, OrderbookEntry>();
        private readonly Dictionary<long, OrderbookEntry> _goodTillCancel = new Dictionary<long, OrderbookEntry>(); // by default all of our orders are of this type
                                                                                                                    // set a 90 day limit, when the time is up remove all goodTillCancel orders
        private readonly Dictionary<long, OrderbookEntry> _intermediateOrCancel = new Dictionary<long, OrderbookEntry>();

        private readonly Lock _ordersLock = new();
        private readonly Lock _goodForDayLock = new();
        private readonly Lock _goodTillCancelLock = new();

        public Orderbook(Security instrument) 
        {
            _instrument = instrument;
        }

        public void addOrder(Order order)
        {
            var baseLimit = new Limit(order.Price);
            addOrder(order, baseLimit, order.isBuySide ? _bidLimits : _askLimits, _orders);
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
            // add special handling if it is of a different type.
        }

        public void removeOrder(CancelOrder cancel)
        {
            if (_orders.TryGetValue(cancel.OrderID, out OrderbookEntry orderbookentry))
            {
                removeOrder(cancel.OrderID, orderbookentry, _orders);
            }
        }

        // check this removeOrder it may not be working properly
        private void removeOrder(long id, OrderbookEntry orderentry, Dictionary<long, OrderbookEntry> orders)
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
                
                if (orderentry.CurrentOrder.isBuySide)
                {
                    _bidLimits.Remove(orderentry.ParentLimit);
                }

                else 
                {
                    _askLimits.Remove(orderentry.ParentLimit);
                }
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

        // also check this, it may also not be working properly
        public void modifyOrder(ModifyOrder modify)
        {
            if (_orders.TryGetValue(modify.OrderID, out OrderbookEntry orderentry))
            {
                removeOrder(modify.cancelOrder());
                addOrder(modify.newOrder(), orderentry.ParentLimit, modify.isBuySide ? _bidLimits : _askLimits, _orders);
            }
        }

        public void ProcessGoodForDayOrders()
        {
            // process the good for day orders
            // handle the FillOrKill, IntermediateOrCancel orders in the matching method 
            // which we will do later

            lock (_ordersLock)
            {
                DateTime now = DateTime.UtcNow;
                // do something
            
                lock (_goodForDayLock)
                {
                    // process all of the GoodForDay orders
                }
            }
        }

        public void DeleteExpiredGoodTillCancels()
        {
            lock (_ordersLock)
            {

                lock (_goodTillCancelLock)
                {
                    // delete expired goodTillCancel orders 
                }
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

        public SortedSet<Limit> getAskLimits()
        {
            return _askLimits;
        }

        public SortedSet<Limit> getBidLimits()
        {
            return _bidLimits;
        }

        public string getSecurityName()
        {
            return _instrument.name;
        }

        public bool canMatch()
        {
        // Determines if a match can happen in this limit orderbook;
        // we need to also check if the limits we're trying to match are null or not
            foreach (var ask in _askLimits)
            {
                foreach (var bid in _bidLimits)
                {
                    if (ask.Price <= bid.Price)
                        return true;
                }
            }
            return false;
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
    }
}