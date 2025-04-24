using TradingServer.Instrument;
using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public class Orderbook: IRetrievalOrderbook, IMatchingOrderbook, IDisposable
    {
        private readonly Security _instrument;
        
        private readonly SortedSet<Limit> _askLimits = new SortedSet<Limit>(AskLimitComparer.comparer);
        private readonly SortedSet<Limit> _bidLimits = new SortedSet<Limit>(BidLimitComparer.comparer);

        private readonly Dictionary<long, OrderbookEntry> _orders = new Dictionary<long, OrderbookEntry>();

        private readonly Dictionary<long, OrderbookEntry> _goodTillCancel = new Dictionary<long, OrderbookEntry>(); // can consider making these just orders, uses less heap memory
        private readonly Dictionary<long, OrderbookEntry> _onMarketOpen = new Dictionary<long, OrderbookEntry>();
        private readonly Dictionary<long, OrderbookEntry> _onMarketClose = new Dictionary<long, OrderbookEntry>();
        private readonly Dictionary<long, CancelOrder> _goodForDay = new Dictionary<long, CancelOrder>();

        private readonly Dictionary<long, StopOrder> _stop = new Dictionary<long, StopOrder>();
        private readonly Dictionary<long, TrailingStopOrder> _trailingStop = new Dictionary<long, TrailingStopOrder>();
        
        private readonly Mutex _orderMutex = new Mutex();
        private readonly Mutex _goodForDayMutex = new Mutex();
        private readonly Mutex _goodTillCancelMutex = new Mutex();

        private readonly Lock _ordersLock = new();
        private readonly Lock _goodForDayLock = new();
        private readonly Lock _goodTillCancelLock = new();
        private readonly Lock _stopLock = new();

        private DateTime now; 
        private Trades _trades;
        private long _greatestTradedPrice = Int32.MinValue;
        private long _lastTradedPrice;

        private bool _disposed = false;
        CancellationTokenSource _ts = new CancellationTokenSource();

        public Orderbook(Security instrument) 
        {
            _instrument = instrument;
            _trades = new Trades();

            _ = Task.Run(() => ProcessAtMarketOpen());
            _ = Task.Run(() => ProcessAtMarketEnd());
            _ = Task.Run(() => ProcessStopOrders());
            _ = Task.Run(() => ProcessTrailingStopOrders());
            _ = Task.Run(() => UpdateGreatestTradedPrice());
        }

        public void addOrder(Order order) // make this public for unit tests to make sure it works ok - otherwise this may be private
        {
            lock (_ordersLock) 
            {
                var baseLimit = new Limit(order.Price);

                if (order.OrderType != OrderTypes.StopLimit || order.OrderType != OrderTypes.StopMarket)
                {
                    throw new InvalidOperationException();
                }

                if (!_orders.TryGetValue(order.OrderID, out OrderbookEntry? orderbookentry)) 
                {
                    addOrder(order, baseLimit, order.isBuySide ? _bidLimits : _askLimits, _orders);
                }

                else if (!_stop.TryGetValue(order.OrderID, out StopOrder? stop) 
                        && order.OrderType == OrderTypes.StopMarket || order.OrderType == OrderTypes.StopLimit)
                {
                    _stop.Add(order.OrderID, (StopOrder) order);
                }

                else if (!_trailingStop.TryGetValue(order.OrderID, out TrailingStopOrder? trailing) && 
                    order.OrderType == OrderTypes.TrailingStop)
                {
                    _trailingStop.Add(order.OrderID, (TrailingStopOrder) order);
                }

                else if (!_onMarketOpen.TryGetValue(order.OrderID, out OrderbookEntry? moo) && 
                    order.OrderType == OrderTypes.MarketOnOpen || order.OrderType == OrderTypes.LimitOnOpen)
                {
                    _onMarketOpen.Add(order.OrderID, new OrderbookEntry(order, baseLimit));
                }

                else if (!_onMarketClose.TryGetValue(order.OrderID, out OrderbookEntry? coo) && 
                    order.OrderType == OrderTypes.MarketOnClose || order.OrderType == OrderTypes.LimitOnClose)
                {
                    _onMarketClose.Add(order.OrderID, new OrderbookEntry(order, baseLimit));
                }
            }
        }
        
        private void addOrder(Order order, Limit baseLimit, SortedSet<Limit> levels, Dictionary<long, OrderbookEntry> orders)
        {
            OrderbookEntry orderbookEntry = new OrderbookEntry(order, baseLimit);

            if (orderbookEntry.CurrentOrder.OrderType == OrderTypes.GoodForDay)
            {
                lock (_goodForDayLock)
                {
                    _goodForDay.Add(order.OrderID, new CancelOrder(order));
                }
            }

            else if (orderbookEntry.CurrentOrder.OrderType == OrderTypes.GoodTillCancel)
            {
                lock (_goodTillCancelLock)
                {
                    _goodTillCancel.Add(order.OrderID, orderbookEntry);
                }
            }

            {
                if (levels.TryGetValue(baseLimit, out Limit? limit) && limit != null)
                {
                    if (limit.head == null)
                    {
                        limit.head = orderbookEntry;
                        limit.tail = orderbookEntry;
                    }

                    else
                    {
                        if (limit.tail != null)
                        {
                            OrderbookEntry tailPointer = limit.tail;
                            tailPointer.next = orderbookEntry;
                            orderbookEntry.previous = tailPointer;
                            limit.tail = orderbookEntry;
                        }
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
        }

        private void removeOrders(List<CancelOrder> cancels)
        {
            lock (_ordersLock)
            {
                foreach (CancelOrder cancel in cancels)
                {
                    if (_orders.TryGetValue(cancel.OrderID, out OrderbookEntry? orderbookentry) && orderbookentry != null)
                    {
                        removeOrder(cancel.OrderID, orderbookentry, _orders);
                        orderbookentry.Dispose();
                    }
                }
            }
        }

        public void removeOrder(CancelOrder cancel)
        {
            lock (_ordersLock)
            {
                if (_orders.TryGetValue(cancel.OrderID, out OrderbookEntry? orderbookentry) && orderbookentry != null)
                {
                    removeOrder(cancel.OrderID, orderbookentry, _orders);
                    orderbookentry.Dispose();
                }

                else if (_stop.TryGetValue(cancel.OrderID, out StopOrder? stop) && stop != null)
                {
                    _stop.Remove(cancel.OrderID);
                    stop.Dispose();
                }

                else if (_trailingStop.TryGetValue(cancel.OrderID, out TrailingStopOrder? trailing_stop) && trailing_stop != null)
                {
                    _trailingStop.Remove(cancel.OrderID);
                    trailing_stop.Dispose();
                }

                else if (_onMarketOpen.TryGetValue(cancel.OrderID, out OrderbookEntry? omo) && omo != null)
                {
                    _onMarketOpen.Remove(cancel.OrderID);
                    omo.Dispose();
                }

                else if (_onMarketClose.TryGetValue(cancel.OrderID, out OrderbookEntry? omc) && omc != null)
                {
                    _onMarketClose.Remove(cancel.OrderID);
                    omc.Dispose();
                }
            }
        }

        private void removeOrder(long id, OrderbookEntry orderentry, Dictionary<long, OrderbookEntry> orders)
        {
            if (orderentry.CurrentOrder.OrderType == OrderTypes.GoodForDay)
            {
                lock (_goodForDayLock)
                {
                    _goodForDay.Remove(id);
                }
            }

            else if (orderentry.CurrentOrder.OrderType == OrderTypes.GoodTillCancel)
            {
                lock (_goodTillCancelLock)
                {
                    _goodTillCancel.Remove(id);
                }
            }

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
                orderentry.Dispose();
            }
        }

        public void modifyOrder(ModifyOrder modify)
        {   
            lock (_ordersLock)
            {
                if (_orders.TryGetValue(modify.OrderID, out OrderbookEntry? orderentry) && orderentry != null)
                {
                    removeOrder(modify.OrderID, orderentry, _orders);
                    //addOrder(modify.newOrder(), orderentry.ParentLimit, modify.isBuySide ? _bidLimits : _askLimits, _orders);
                    matchIncoming(modify.newOrder());
                }
            }
        }

        private void DeleteExpiredGoodTillCancel()
        {
            List<CancelOrder> goodTillCancelOrders = new List<CancelOrder>();

            foreach (var order in _goodTillCancel)
            {
                if ((DateTime.UtcNow - order.Value.CreationTime).TotalDays >= 90)
                {
                    goodTillCancelOrders.Add(new CancelOrder(order.Value.CurrentOrder));
                }

                removeOrders(goodTillCancelOrders);
            }
        }

        private async Task ProcessAtMarketEnd()
        {
            while (true)
            {
                if (_ts.IsCancellationRequested)
                {
                    return;
                }

                DateTime currentTime = DateTime.Now;

                if (currentTime.Hour >= 16)
                {
                    DateTime tomorrow = currentTime.AddDays(1);
                    DateTime nextTradingDayStart = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 9, 30, 0);
                    now = nextTradingDayStart;

                    TimeSpan closed = nextTradingDayStart - DateTime.Now;

                    try
                    {
                        removeOrders(_goodForDay.Values.ToList());

                        DeleteExpiredGoodTillCancel();
                        ProcessOnMarketEndOrders(); 

                        _orderMutex.WaitOne();

                        _goodForDayMutex.WaitOne();
                        _goodTillCancelMutex.WaitOne();

                        Thread.Sleep(closed);
                    }

                    finally
                    {
                        _orderMutex.ReleaseMutex();

                        _goodForDayMutex.ReleaseMutex();
                        _goodTillCancelMutex.ReleaseMutex();
                    }
                }
                
                else 
                {
                    now = currentTime;
                }

                await Task.Delay(200, _ts.Token);

                if (_ts.IsCancellationRequested)
                {
                    return;
                }
            }
        }

        private async Task ProcessAtMarketOpen()
        {
            while (true)
            {
                if (_ts.IsCancellationRequested)
                {
                    return;
                }

                if (now.Hour == 9 && now.Minute == 30)
                {
                    lock (_ordersLock)
                    {
                        foreach (var order in _onMarketOpen)
                        {
                            var orderEntry = order.Value;
                            var id = order.Value.CurrentOrder.OrderID;

                            match(order.Value.CurrentOrder);
                            _onMarketOpen.Remove(id);

                            orderEntry.Dispose();
                        }
                    }
                }

                if (_ts.IsCancellationRequested)
                {
                    return;
                }
                
                await Task.Delay(200, _ts.Token);
            }
            // process at the start of the day (9:30 local time), 
            // must be in before 9:28 AM local time (or throw a error otherwise)
        }

        private void ProcessOnMarketEndOrders()
        {
            foreach (var order in _onMarketClose)
            {
                var current = order.Value;
                match(current.CurrentOrder);

                _onMarketClose.Remove(current.CurrentOrder.OrderID);
                current.Dispose();
            }
            // process these at the end of the day; we should specify somewhere
            // that they need to be placed before 3:50 PM or throw a error otherwise
        }

        private async Task ProcessStopOrders()
        {
            // only do this one while the market is open
            while (true)
            {
                if (_ts.IsCancellationRequested)
                {
                    return;
                }

                DateTime now = DateTime.Now;
                TimeSpan currentTime = now.TimeOfDay;
                TimeSpan marketOpen = new TimeSpan(9, 30, 0);
                TimeSpan marketEnd = new TimeSpan(4, 0, 0);

                if (currentTime >= marketOpen && currentTime <= marketEnd)
                {
                    foreach (var order in _stop)
                    {
                        var tempOrder = order.Value;

                        if (tempOrder.isBuySide)
                        {
                            if (_lastTradedPrice <= order.Value.Price)
                            {
                                Order activate = order.Value.activate();
                                match(activate);

                                if (order.Value.CurrentQuantity > 0)
                                {
                                    addOrder(order.Value);
                                }

                                else
                                {
                                    order.Value.Dispose(); // check if this is doing something wacky
                                }

                                _stop.Remove(tempOrder.OrderID);
                            }
                        }

                        else
                        {
                            if (_lastTradedPrice >= order.Value.Price)
                            {
                                Order activated = order.Value.activate();
                                match(activated);

                                if (order.Value.CurrentQuantity > 0) // also check that this is ok
                                {
                                    addOrder(order.Value);
                                }
                                
                                else 
                                {
                                    order.Value.Dispose(); // check if this is doing something wacky
                                }

                                _stop.Remove(tempOrder.OrderID);
                            }     
                        }
                    }
                }

                if (_ts.IsCancellationRequested)
                {
                    return;
                }

                await Task.Delay(200, _ts.Token);
            }
        }

        private async Task UpdateGreatestTradedPrice()
        {
            while (true) // though we should only do this at appropriate times in the day
            {
                DateTime now = DateTime.Now;
                TimeSpan currentTime = now.TimeOfDay;
                TimeSpan marketOpen = new TimeSpan(9, 30, 0);
                TimeSpan marketEnd = new TimeSpan(4, 0, 0);

                if (_ts.IsCancellationRequested)
                {
                    return;
                }

                if (_trades.result.Count > 0 && currentTime <= marketEnd && currentTime >= marketOpen)
                {
                    var lastTrade = _trades.result[_trades.result.Count - 1];
                    if (lastTrade.tradedPrice > _greatestTradedPrice)
                    {
                        _greatestTradedPrice = lastTrade.tradedPrice;
                    }
                }

                if (_ts.IsCancellationRequested)
                {
                    return;
                }

                await Task.Delay(200, _ts.Token);
            }
        }

        private async Task ProcessTrailingStopOrders()
        {
            while (true)
            {
                if (_ts.IsCancellationRequested)
                {
                    return;
                }

                DateTime now = DateTime.Now;
                TimeSpan currentTime = now.TimeOfDay;
                TimeSpan marketOpen = new TimeSpan(9, 30, 0);
                TimeSpan marketEnd = new TimeSpan(4, 0, 0);

                if (currentTime >= marketOpen && currentTime <= marketEnd)
                {
                    lock (_ordersLock) 
                    {
                        foreach (var trail in _trailingStop)
                        {
                            var trailstop = trail.Value;

                            if (trailstop.isBuySide)
                            {
                                if (_lastTradedPrice <= trailstop.StopPrice)
                                {
                                    Order activated = trailstop.activate();
                                    matchIncoming(activated);

                                    trail.Value.Dispose();
                                    _trailingStop.Remove(trailstop.OrderID);
                                }

                                else if (_greatestTradedPrice > trailstop.currentMaxPrice)
                                {
                                    trail.Value.currentMaxPrice = _greatestTradedPrice;
                                }
                            }

                            else 
                            {
                                if (_lastTradedPrice >= trailstop.StopPrice)
                                {
                                   Order activated = trailstop.activate();
                                   matchIncoming(activated);

                                    trail.Value.Dispose();
                                    _trailingStop.Remove(trailstop.OrderID);
                                }
                            }
                        }
                    }
                }

                if (_ts.IsCancellationRequested)
                {
                    return;
                }

                await Task.Delay(200);
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

        public long currentPrice()
        {
            return _lastTradedPrice;
        }

        public bool canFill(Order order)
        {
            if (order.isBuySide)
            {
                uint askQuantity = 0;

                foreach (var ask in _askLimits)
                {
                    if (ask.Price <= order.Price)
                    {
                        OrderbookEntry askHead = ask.head;

                        while (askHead != null)
                        {
                            askQuantity += askHead.CurrentOrder.CurrentQuantity;
                            askHead = askHead.next;
                        }
                    }
                }
                return askQuantity >= order.CurrentQuantity;
            }

            else
            {
                uint bidQuantity = 0;
                
                foreach (var bid in _bidLimits)
                {
                    if (bid.Price >= order.Price)
                    {
                        OrderbookEntry bidHead = bid.head;

                        while (bidHead != null)
                        {
                            bidQuantity += bidHead.CurrentOrder.CurrentQuantity;
                            bidHead = bidHead.next;
                        }
                    }
                }

                return bidQuantity >= order.CurrentQuantity;
            }
        }

        public Trades match(Order order) // redesign the whole thing
        {
            Trades result = new Trades();

            if (order.isBuySide)
            {
                foreach (var ask in _askLimits)
                {
                    if (ask.Price <= order.Price)
                    {
                        // work out new logic here, this needs to be fixed
                        // needs to check for duplicated/corrupted refs
                    }
                }
            }

            else 
            {
                foreach (var bid in _bidLimits)
                {
                    if (bid.Price >= order.Price)
                    {
                        // work out new logic here, this needs to be fixed
                        // need to ensure safety against duplicated/corrupted refs
                    }
                }
            }

            return result;
        }

        public Trades matchIncoming(Order order) 
        {   
            Lock _orderLock = new(); 

            Trades result = new Trades();

            lock (_orderLock)
            {
                if (order.OrderType == OrderTypes.StopLimit || order.OrderType == OrderTypes.StopMarket)
                {
                    throw new InvalidOperationException("In general, we attempt to match non-stop orders when they come in "
                    + "- we add them to internal queues instead with addOrder.");
                }

                if (order.OrderType == OrderTypes.FillOrKill)
                {
                    if (canFill(order))
                    {
                        result = match(order);
                        order.Dispose();
                    }
                }

                else if (order.OrderType == OrderTypes.FillAndKill)
                {
                    result = match(order);
                    order.Dispose();
                } 

                else if (order.OrderType == OrderTypes.Market)
                {
                    result = match(order);
                    order.Dispose();
                }

                else if (order.OrderType == OrderTypes.PostOnly)
                {
                    if (!canFill(order))
                        addOrder(order);
                }

                else if (order.OrderType == OrderTypes.StopMarket || order.OrderType == OrderTypes.StopLimit)
                {
                    addOrder(order);
                }

                else if (order.OrderType == OrderTypes.TrailingStop)
                {
                    addOrder(order);
                }

                else if (order.OrderType == OrderTypes.LimitOnClose || order.OrderType == OrderTypes.MarketOnClose)
                {
                    addOrder(order);
                }

                else if (order.OrderType == OrderTypes.LimitOnOpen || order.OrderType == OrderTypes.MarketOnOpen)
                {
                    addOrder(order); // add to internal order queue
                }

                else 
                {
                    result = match(order);
                
                    if (order.CurrentQuantity > 0)
                    {
                        addOrder(order);
                    }

                    else 
                    {
                        order.Dispose();
                    }
                }

                return result;
            }
        }

        public void processIncoming(Order order) // review this later, is this necessary?
        {
            if (order.OrderType == OrderTypes.StopLimit || order.OrderType == OrderTypes.StopMarket)
            {
                addOrder(order);
            }

            else 
            {
                matchIncoming(order);
            }
        }

        public OrderbookSpread spread()
        {
            long? bestAsk = null, bestBid = null;
            if (_askLimits.Any() && !_askLimits.Min.isEmpty)
            {
                bestAsk = _askLimits.Min.Price;
            }

            if (_bidLimits.Any() && !_bidLimits.Min.isEmpty)
            {
                bestBid = _askLimits.Max.Price;
            }
            return new OrderbookSpread(bestBid, bestAsk);
        }

        ~Orderbook()
        {
            Dispose();
        }

        public void Dispose() 
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
                _ts.Cancel();
                _ts.Dispose();
            }
        }
    }
}