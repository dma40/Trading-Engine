
using System.Threading.Tasks;
using TradingServer.Instrument;
using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public class Orderbook: IRetrievalOrderbook
    {
        private readonly Security _instrument;
        
        private readonly SortedSet<Limit> _askLimits = new SortedSet<Limit>(AskLimitComparer.comparer);
        private readonly SortedSet<Limit> _bidLimits = new SortedSet<Limit>(BidLimitComparer.comparer);

        private readonly Dictionary<long, OrderbookEntry> _orders = new Dictionary<long, OrderbookEntry>();
        private readonly Dictionary<long, OrderbookEntry> _goodForDay = new Dictionary<long, OrderbookEntry>();
        private readonly Dictionary<long, OrderbookEntry> _fillOrKill = new Dictionary<long, OrderbookEntry>();
        private readonly Dictionary<long, OrderbookEntry> _goodTillCancel = new Dictionary<long, OrderbookEntry>(); 
        private readonly Dictionary<long, OrderbookEntry> _fillAndKill = new Dictionary<long, OrderbookEntry>();
        private readonly Dictionary<long, OrderbookEntry> _market = new Dictionary<long, OrderbookEntry>();

        private readonly Mutex _orderMutex = new Mutex();
        private readonly Mutex _goodForDayMutex = new Mutex();
        private readonly Mutex _goodTillCancelMutex = new Mutex();
        private readonly Mutex _fillOrKillMutex = new Mutex();
        private readonly Mutex _fillAndKillMutex = new Mutex();
        private readonly Mutex _marketMutex = new Mutex();

        private readonly Lock _orderLock = new();
        private readonly Lock _goodForDayLock = new();
        private readonly Lock _goodTillCancelLock = new();
        private readonly Lock _fillOrKillLock = new();
        private readonly Lock _fillAndKillLock = new();
        private readonly Lock _marketLock = new();


        private readonly Thread _goodForDayThread;
        private DateTime now; 

        private bool _disposed = false;
        CancellationTokenSource _ts = new CancellationTokenSource();

        public Orderbook(Security instrument) 
        {
            _instrument = instrument;
            _goodForDayThread = new Thread(() => ProcessGoodForDay().GetAwaiter().GetResult());
            // _goodForDayThread.Start();
        }

        public void addOrder(Order order)
        {
            var baseLimit = new Limit(order.Price);
            addOrder(order, baseLimit, order.isBuySide ? _bidLimits : _askLimits, _orders);
        }
        // maybe add ask, bid sides for orders? maybe that would be helpful

        private void addOrder(Order order, Limit baseLimit, SortedSet<Limit> levels, Dictionary<long, OrderbookEntry> orders)
        {
            OrderbookEntry orderbookEntry = new OrderbookEntry(order, baseLimit);

            if (orderbookEntry.CurrentOrder.OrderType == OrderTypes.FillAndKill)
            {
                _fillAndKillMutex.WaitOne();

                try
                {
                    _fillAndKill.Add(order.OrderID, orderbookEntry);
                }

                finally
                {
                    _fillAndKillMutex.ReleaseMutex();
                }
            }

            else if (orderbookEntry.CurrentOrder.OrderType == OrderTypes.FillOrKill)
            {
                _fillOrKillMutex.WaitOne();

                try
                {
                    _fillOrKill.Add(order.OrderID, orderbookEntry);
                }

                finally
                {
                    _fillOrKillMutex.ReleaseMutex();
                }
            }

            else if (orderbookEntry.CurrentOrder.OrderType == OrderTypes.GoodForDay)
            {
                _goodForDayMutex.WaitOne();

                try
                {
                    _goodForDay.Add(order.OrderID, orderbookEntry);
                }

                finally
                {
                    _goodForDayMutex.ReleaseMutex();
                }
            }

            else if (orderbookEntry.CurrentOrder.OrderType == OrderTypes.GoodTillCancel)
            {
                _goodTillCancelMutex.WaitOne();

                try
                {
                    _goodTillCancel.Add(order.OrderID, orderbookEntry);
                }

                finally
                {
                    _goodTillCancelMutex.ReleaseMutex();
                }
            }

            else if (orderbookEntry.CurrentOrder.OrderType == OrderTypes.Market)
            {
                _marketMutex.WaitOne();

                try
                {
                    _market.Add(order.OrderID, orderbookEntry);
                }

                finally
                {
                    _marketMutex.ReleaseMutex();
                }
            }

            _orderMutex.WaitOne();

            try
            {
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

            finally
            {
                _orderMutex.ReleaseMutex();
            }
            // add special handling if it is of a different type.
        }

        public void removeOrder(CancelOrder cancel)
        {
            if (_orders.TryGetValue(cancel.OrderID, out OrderbookEntry orderbookentry))
            {
                removeOrder(cancel.OrderID, orderbookentry, _orders);
            }
        }

        // check this removeOrder it may not be working properly; wacky things may be happening
        private void removeOrder(long id, OrderbookEntry orderentry, Dictionary<long, OrderbookEntry> orders)
        {
            if (orderentry.CurrentOrder.OrderType == OrderTypes.FillAndKill)
            {
                _fillAndKillMutex.WaitOne();

                try
                {
                    _fillAndKill.Remove(id);
                }

                finally
                {
                    _fillAndKillMutex.ReleaseMutex();
                }
            }

            else if (orderentry.CurrentOrder.OrderType == OrderTypes.FillOrKill)
            {
                _fillOrKillMutex.WaitOne(); // do some investigating as to whether lock or mutex is better for this case

                try
                {
                    _fillOrKill.Remove(id);
                }

                finally
                {
                    _fillOrKillMutex.ReleaseMutex();
                }
            }

            else if (orderentry.CurrentOrder.OrderType == OrderTypes.GoodForDay)
            {
                _goodForDayMutex.WaitOne();

                try
                {
                    _goodForDay.Remove(id);
                }

                finally
                {
                    _goodForDayMutex.ReleaseMutex();
                }
            }

            else if (orderentry.CurrentOrder.OrderType == OrderTypes.GoodTillCancel)
            {
                _goodTillCancelMutex.WaitOne();

                try
                {
                    _goodTillCancel.Remove(id);
                }

                finally
                {
                    _goodTillCancelMutex.ReleaseMutex();
                }
            }

            else if (orderentry.CurrentOrder.OrderType == OrderTypes.Market)
            {
                _marketMutex.WaitOne();

                try
                {
                    _market.Remove(id);
                }

                finally
                {
                    _marketMutex.ReleaseMutex();
                }
            }

            _orderMutex.WaitOne();

            try
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

            finally
            {
                _orderMutex.ReleaseMutex();
            }
        }

        // also check this, it may also not be working properly
        public void modifyOrder(ModifyOrder modify) // side is never modified by our methods so we don't restrict access in this method
        {
            _orderMutex.WaitOne();
            
            try
            {
                if (_orders.TryGetValue(modify.OrderID, out OrderbookEntry orderentry))
                {
                    removeOrder(modify.cancelOrder());
                    addOrder(modify.newOrder(), orderentry.ParentLimit, modify.isBuySide ? _bidLimits : _askLimits, _orders);
                }
            }

            finally
            {
                _orderMutex.ReleaseMutex();
            }
        }

        private void DeleteExpiredGoodTillCancel()
        {
            // _goodTillCancelMutex.WaitOne();

            // try
            foreach (var order in _goodTillCancel)
            {
                if ((DateTime.UtcNow - order.Value.CreationTime).TotalDays >= 90)
                {
                        removeOrder(new CancelOrder(order.Value.CurrentOrder));
                }
            }
                // delete expired goodTillCancel orders; maybe this should be called periodically, at the same time with 
                // ProcessGoodForDay which is done at the end of the trading day. Also maybe modify so that we don't catch/release the same mutex 
                // multiple times
            

            // finally
            // {
            //     _goodTillCancelMutex.ReleaseMutex();
            // } 
        }

        private async Task ProcessGoodForDay()
        {
            // should it immediately roll over into the next day, or shut down the orderbook at 4PM UTC?
            while (true) 
            {
                DateTime currentTime = DateTime.UtcNow;

                if (currentTime.Hour >= 16)
                {
                    DateTime tomorrow = currentTime.AddDays(1);
                    DateTime nextTradingDayStart = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 9, 30, 0);
                    now = nextTradingDayStart;

                    TimeSpan closed = nextTradingDayStart - DateTime.Now;

                    _orderMutex.WaitOne();
                    _goodForDayMutex.WaitOne();

                    _fillAndKillMutex.WaitOne();
                    _fillOrKillMutex.WaitOne();
                    _goodTillCancelMutex.WaitOne();
                    _marketMutex.WaitOne();

                    try
                    {
                        foreach (var order in _goodForDay)
                        {
                            // modify to have better performance
                            removeOrder(new CancelOrder(order.Value.CurrentOrder));
                        }

                        DeleteExpiredGoodTillCancel(); // make sure we're not catching + releasing lock twice
                        Thread.Sleep(closed);
                    }

                    finally
                    {
                        _orderMutex.ReleaseMutex();
                        _goodForDayMutex.ReleaseMutex();

                        _fillAndKillMutex.ReleaseMutex();
                        _fillOrKillMutex.ReleaseMutex();
                        _marketMutex.ReleaseMutex();
                    }

                    // Monitor.Wait(_orderMutex, closed);
                    
                }
                
                else 
                {
                    now = currentTime;
                }

                await Task.Delay(5000);

                // _orderMutex.WaitOne();
                /*
                _goodForDayMutex.WaitOne();

                try
                {
                    {
                        foreach (var order in _goodForDay)
                        {
                            // modify to have better performance
                            removeOrder(new CancelOrder(order.Value.CurrentOrder));
                        }

                        DeleteExpiredGoodTillCancel(); // make sure we're not catching + releasing lock twice!
                    }
                }

                finally
                {
                    // _orderMutex.ReleaseMutex();
                    _goodForDayMutex.ReleaseMutex();
                }
                */
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

        ~Orderbook()
        {
            Dispose(false);
        }

        public void Dispose() // do something like this in Orderbook, dispose of this object when we don't want it anymore
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool dispose) 
        {
            if (_disposed) 
            { 
                return;
            }

            _disposed = true;

            if (dispose) 
            {
                _goodForDayThread.Join(); // make sure nothing can call while this is being deleted
                _ts.Cancel();
                _ts.Dispose();
            }
        }
    }
}