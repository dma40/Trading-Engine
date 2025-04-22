using TradingServer.Instrument;
using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public class Orderbook: IRetrievalOrderbook, IDisposable
    {
        private readonly Security _instrument;
        
        private readonly SortedSet<Limit> _askLimits = new SortedSet<Limit>(AskLimitComparer.comparer);
        private readonly SortedSet<Limit> _bidLimits = new SortedSet<Limit>(BidLimitComparer.comparer);

        private readonly Dictionary<long, OrderbookEntry> _orders = new Dictionary<long, OrderbookEntry>();
        private readonly Dictionary<long, OrderbookEntry> _goodTillCancel = new Dictionary<long, OrderbookEntry>(); 
        private readonly Dictionary<long, OrderbookEntry> _onMarketOpen = new Dictionary<long, OrderbookEntry>();
        private readonly Dictionary<long, OrderbookEntry> _onMarketClose = new Dictionary<long, OrderbookEntry>();
        private readonly Dictionary<long, CancelOrder> _goodForDay = new Dictionary<long, CancelOrder>();

        private readonly Mutex _orderMutex = new Mutex();
        private readonly Mutex _goodForDayMutex = new Mutex();
        private readonly Mutex _goodTillCancelMutex = new Mutex();

        private readonly Lock _ordersLock = new();
        private readonly Lock _goodForDayLock = new();
        private readonly Lock _goodTillCancelLock = new();

        private DateTime now; 

        private bool _disposed = false;
        CancellationTokenSource _ts = new CancellationTokenSource();

        public Orderbook(Security instrument) 
        {
            _instrument = instrument;

            _ = Task.Run(() => ProcessAtMarketOpen());
            _ = Task.Run(() => ProcessAtMarketEnd());
            _ = Task.Run(() => ProcessStopOrders());
        }

        public void addOrder(Order order)
        {
            lock (_ordersLock) 
            {
                if (!_orders.TryGetValue(order.OrderID, out OrderbookEntry? orderbookentry) && orderbookentry != null) 
                {
                    var baseLimit = new Limit(order.Price);
                    addOrder(order, baseLimit, order.isBuySide ? _bidLimits : _askLimits, _orders);
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

        public void removeOrders(List<CancelOrder> cancels)
        {
            lock (_ordersLock)
            {
                foreach (CancelOrder cancel in cancels)
                {
                    if (_orders.TryGetValue(cancel.OrderID, out OrderbookEntry? orderbookentry) && orderbookentry != null)
                    {
                        removeOrder(cancel.OrderID, orderbookentry, _orders);
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
            }
        }

        public void modifyOrder(ModifyOrder modify)
        {   
            lock (_ordersLock)
            {
                if (_orders.TryGetValue(modify.OrderID, out OrderbookEntry? orderentry) && orderentry != null)
                {
                    removeOrder(modify.OrderID, orderentry, _orders);
                    addOrder(modify.newOrder(), orderentry.ParentLimit, modify.isBuySide ? _bidLimits : _askLimits, _orders);
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

        private async Task ProcessAtMarketStart()
        {
            while (true)
            {
                await Task.Delay(200);
            }
        }

        private async Task ProcessAtMarketOpen()
        {
            await Task.Delay(200);
            // process at the start of the day (9:30 local time)
        }

        private void ProcessOnMarketEndOrders()
        {
            // await Task.Delay(200);
            // process these at the end of the day
        }

        private async Task ProcessStopOrders()
        {
            // only do this one while the market is open
            await Task.Delay(200);
            // this method should check all existing stop loss orders 
            // and see if one of them can match
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

        public Trades fill(Order order)
        {
            Trades result = new Trades();

            if (order.isBuySide)
            {
                foreach (var ask in _askLimits)
                {
                    if (ask.Price <= order.Price)
                    {
                        OrderbookEntry askPtr = ask.head;

                        while (askPtr != null)
                        {
                            if (askPtr.CurrentOrder.CurrentQuantity > order.CurrentQuantity)
                            {
                                OrderRecord incoming = new OrderRecord(order.OrderID, order.CurrentQuantity, 0, 
                                                                    order.Price, order.Price, true, 
                                                                    order.Username, order.SecurityID, 0, 0);
                                OrderRecord resting = new OrderRecord(askPtr.CurrentOrder.OrderID, askPtr.CurrentOrder.CurrentQuantity, 
                                                                    askPtr.CurrentOrder.CurrentQuantity - order.CurrentQuantity, 
                                                                    askPtr.CurrentOrder.Price, order.Price, false, 
                                                                    askPtr.CurrentOrder.Username, askPtr.CurrentOrder.SecurityID, 
                                                                    askPtr.queuePosition(), askPtr.queuePosition());

                                askPtr.CurrentOrder.DecreaseQuantity(order.CurrentQuantity);
                                order.DecreaseQuantity(order.CurrentQuantity); 

                                Trade transaction = new Trade(incoming, resting);

                                result.addTransaction(transaction);
                                break;
                            }

                            else 
                            {
                                uint quantity = askPtr.CurrentOrder.CurrentQuantity;

                                OrderRecord incoming = new OrderRecord(order.OrderID, order.CurrentQuantity, 
                                                                    order.CurrentQuantity - askPtr.CurrentOrder.CurrentQuantity,
                                                                    order.Price, askPtr.CurrentOrder.Price, 
                                                                    true, order.Username, order.SecurityID, 0, 0);
                                OrderRecord resting = new OrderRecord(askPtr.CurrentOrder.OrderID, askPtr.CurrentOrder.CurrentQuantity, 0, 
                                                                askPtr.CurrentOrder.Price, askPtr.CurrentOrder.Price, false, 
                                                                askPtr.CurrentOrder.Username, 
                                                                askPtr.CurrentOrder.SecurityID, askPtr.queuePosition(), 0);

                                askPtr.CurrentOrder.DecreaseQuantity(quantity); 
                                order.DecreaseQuantity(quantity);

                                Trade transaction = new Trade(incoming, resting);

                                result.addTransaction(transaction);

                                if (askPtr.next != null)
                                {
                                    askPtr = askPtr.next;
                                    removeOrder(askPtr.previous.CurrentOrder.OrderID, askPtr.previous, _orders);    
                                }

                                else 
                                {
                                    askPtr = askPtr.next;
                                }
                            }
                        }
                    }
                }
            }

            else 
            {
                foreach (var bid in _bidLimits)
                {
                    if (bid.Price >= order.Price)
                    {
                        OrderbookEntry bidPtr = bid.head;

                        while (bidPtr != null)
                        {
                            if (bidPtr.CurrentOrder.CurrentQuantity > order.CurrentQuantity)
                            {
                                OrderRecord incoming = new OrderRecord(order.OrderID, order.CurrentQuantity, 0, 
                                                                    order.Price, bidPtr.CurrentOrder.Price, true, 
                                                                    order.Username, order.SecurityID, 0, 0);
                                OrderRecord resting = new OrderRecord(bidPtr.CurrentOrder.OrderID, bidPtr.CurrentOrder.CurrentQuantity, 
                                                                    bidPtr.CurrentOrder.CurrentQuantity - order.CurrentQuantity, 
                                                                    bidPtr.CurrentOrder.Price, order.Price, true, 
                                                                    bidPtr.CurrentOrder.Username, bidPtr.CurrentOrder.SecurityID, 
                                                                    bidPtr.queuePosition(), bidPtr.queuePosition());

                                bidPtr.CurrentOrder.DecreaseQuantity(order.CurrentQuantity);
                                order.DecreaseQuantity(order.CurrentQuantity); 

                                Trade transaction = new Trade(incoming, resting);
                                result.addTransaction(transaction);

                                break;
                            }

                            else 
                            {
                                OrderRecord incoming = new OrderRecord(order.OrderID, order.CurrentQuantity, order.CurrentQuantity - bidPtr.CurrentOrder.CurrentQuantity, 
                                                                        order.Price, bidPtr.CurrentOrder.Price, true, 
                                                                        order.Username, order.SecurityID, 0, 0);
                                OrderRecord resting = new OrderRecord(bidPtr.CurrentOrder.OrderID, bidPtr.CurrentOrder.CurrentQuantity, 0, 
                                                                        bidPtr.CurrentOrder.Price, bidPtr.CurrentOrder.Price, true, 
                                                                        bidPtr.CurrentOrder.Username, bidPtr.CurrentOrder.SecurityID, 
                                                                        bidPtr.queuePosition(), 0);

                                uint quantity = bidPtr.CurrentOrder.CurrentQuantity;

                                bidPtr.CurrentOrder.DecreaseQuantity(quantity); 
                                order.DecreaseQuantity(quantity);

                                Trade transaction = new Trade(incoming, resting);
                                result.addTransaction(transaction);

                                if (bidPtr.next != null)
                                {
                                    bidPtr = bidPtr.next;
                                    removeOrder(bidPtr.previous.CurrentOrder.OrderID, bidPtr.previous, _orders);
                                }

                                else 
                                {
                                    bidPtr = bidPtr.next;
                                }
                            }
                        }
                    }
                }
            }

            return result;
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