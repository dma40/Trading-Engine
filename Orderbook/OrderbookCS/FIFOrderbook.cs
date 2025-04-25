using TradingServer.Instrument;
using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class MatchingOrderbook: Orderbook, IMatchingOrderbook, IDisposable
    {
        private readonly Dictionary<long, StopOrder> _stop = new Dictionary<long, StopOrder>();
        private readonly Dictionary<long, TrailingStopOrder> _trailingStop = new Dictionary<long, TrailingStopOrder>();

        private DateTime now;
        private static readonly TimeSpan marketOpen = new TimeSpan(9, 30, 0);
        private static readonly TimeSpan marketEnd = new TimeSpan(16, 0, 0);
        
        private Trades _trades;
        private long _greatestTradedPrice = Int32.MinValue;
        private long _lastTradedPrice;

        private readonly Lock _ordersLock = new();
        private readonly Lock _goodForDayLock = new();
        private readonly Lock _goodTillCancelLock = new();
        private readonly Lock _stopLock = new();

        private readonly Mutex _orderMutex = new Mutex();
        private readonly Mutex _goodForDayMutex = new Mutex();
        private readonly Mutex _goodTillCancelMutex = new Mutex();

        private readonly Dictionary<long, Order> _onMarketOpen = new Dictionary<long, Order>();
        private readonly Dictionary<long, Order> _onMarketClose = new Dictionary<long, Order>();

        public MatchingOrderbook(Security security): base(security)
        {
           _trades = new Trades();

           _ = Task.Run(() => ProcessStopOrders());
           _ = Task.Run(() => ProcessTrailingStopOrders());
           _ = Task.Run(() => ProcessAtMarketOpen());
           _ = Task.Run(() => ProcessAtMarketEnd());
           _ = Task.Run(() => UpdateGreatestTradedPrice());
        }

        public new Trades match(Order order) 
        {   
            Lock _orderLock = new(); 

            Trades result = new Trades();

            lock (_orderLock)
            {
                if (order.OrderType == OrderTypes.FillOrKill)
                {
                    if (canFill(order))
                    {
                        result = base.match(order);
                        order.Dispose();
                    }
                }

                else if (order.OrderType == OrderTypes.FillAndKill)
                {
                    result = base.match(order);
                    order.Dispose();
                } 

                else if (order.OrderType == OrderTypes.Market)
                {
                    result = base.match(order);
                    order.Dispose();
                }

                else if (order.OrderType == OrderTypes.PostOnly)
                {
                    if (!canFill(order))
                        base.addOrder(order);
                }

                else 
                {
                    result = base.match(order);
                
                    if (order.CurrentQuantity > 0)
                    {
                        base.addOrder(order);
                    }

                    else 
                    {
                        order.Dispose();
                    }
                }

                return result;
            }
        }

        public sealed override void addOrder(Order order)
        { 
            if (order.OrderType == OrderTypes.StopLimit || order.OrderType == OrderTypes.StopMarket)
            {
                _stop.Add(order.OrderID, (StopOrder) order);
            }

            else if (order.OrderType == OrderTypes.TrailingStop)
            {
                _trailingStop.Add(order.OrderID, (TrailingStopOrder) order);
            }

            else if (order.OrderType == OrderTypes.LimitOnClose || order.OrderType == OrderTypes.MarketOnClose)
            {
                _onMarketClose.Add(order.OrderID, order);
            }

            else if (order.OrderType == OrderTypes.LimitOnOpen || order.OrderType == OrderTypes.MarketOnOpen)
            {
                _onMarketOpen.Add(order.OrderID, order);
            }

            else 
            {
                match(order);
            }
        }

        public sealed override void modifyOrder(ModifyOrder modify)
        {
            removeOrder(modify.cancelOrder());
            addOrder(modify.newOrder());
        }

        public sealed override void removeOrder(CancelOrder cancel)
        {
            if (cancel.OrderType == OrderTypes.StopLimit || cancel.OrderType == OrderTypes.StopMarket)
            {
                _stop.Remove(cancel.OrderID);
            }

            else if (cancel.OrderType == OrderTypes.TrailingStop)
            {
                _trailingStop.Remove(cancel.OrderID);
            }

            else if (cancel.OrderType == OrderTypes.LimitOnClose || cancel.OrderType == OrderTypes.MarketOnClose)
            {
                _onMarketClose.Remove(cancel.OrderID);
            }

            else if (cancel.OrderType == OrderTypes.LimitOnOpen || cancel.OrderType == OrderTypes.MarketOnOpen)
            {
                _onMarketOpen.Remove(cancel.OrderID);
            }

            else 
            {
                base.removeOrder(cancel);
            }
        }

        protected async Task ProcessStopOrders()
        {
            while (true)
            {
                if (_ts.IsCancellationRequested)
                {
                    return;
                }

                TimeSpan currentTime = now.TimeOfDay;

                if (currentTime >= marketOpen && currentTime <= marketEnd)
                {
                    foreach (var order in _stop)
                    {
                        var tempOrder = order.Value;

                        if (tempOrder.isBuySide)
                        {
                            if (_lastTradedPrice <= order.Value.StopPrice)
                            {
                                Order activate = order.Value.activate();
                                match(activate);

                                if (order.Value.CurrentQuantity > 0)
                                {
                                    addOrder(activate);
                                }

                                else
                                {
                                    activate.Dispose();
                                    order.Value.Dispose(); 
                                }

                                _stop.Remove(tempOrder.OrderID);
                            }
                        }

                        else
                        {
                            if (_lastTradedPrice >= order.Value.StopPrice)
                            {
                                Order activated = order.Value.activate();
                                match(activated);

                                if (order.Value.CurrentQuantity > 0)
                                {
                                    addOrder(activated);
                                }
                                
                                else 
                                {
                                    order.Value.Dispose();
                                    activated.Dispose();
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

        protected async Task UpdateGreatestTradedPrice()
        {
            while (true)
            {
                TimeSpan currentTime = now.TimeOfDay;

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

        protected async Task ProcessTrailingStopOrders()
        {
            while (true)
            {
                if (_ts.IsCancellationRequested)
                {
                    return;
                }

                TimeSpan currentTime = now.TimeOfDay;

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
                                    match(activated);

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
                                   match(activated);

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

                await Task.Delay(200, _ts.Token);
            }
        }

        protected sealed override async Task ProcessAtMarketEnd()
        {
            while (true)
            {
                if (_ts.IsCancellationRequested)
                {
                    return;
                }

                DateTime currentTime = DateTime.Now;

                if (currentTime.TimeOfDay >= marketEnd)
                {
                    DateTime tomorrow = currentTime.AddDays(1);
                    DateTime nextTradingDayStart = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 9, 30, 0);
                    TimeSpan closed = nextTradingDayStart - DateTime.Now;

                    try
                    {
                        DeleteGoodForDayOrders();
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

                await Task.Delay(200, _ts.Token);

                if (_ts.IsCancellationRequested)
                {
                    return;
                }
            }
        }

        protected async Task ProcessAtMarketOpen()
        {
            while (true)
            {
                if (_ts.IsCancellationRequested)
                {
                    return;
                }

                if (now.TimeOfDay == marketOpen)
                {
                    lock (_ordersLock)
                    {
                        foreach (var order in _onMarketOpen)
                        {
                            var orderEntry = order.Value;
                            var id = order.Value.OrderID;

                            match(order.Value);
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
        }

        protected void ProcessOnMarketEndOrders()
        {
            foreach (var order in _onMarketClose)
            {
                var current = order.Value;
                match(current);

                _onMarketClose.Remove(current.OrderID);
                current.Dispose();
            }
        }

        public long currentPrice()
        {
            return _lastTradedPrice;
        }

        ~MatchingOrderbook()
        {
            Dispose();
        }

        public new void Dispose() 
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected sealed override void Dispose(bool dispose) 
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

        private readonly CancellationTokenSource _ts = new CancellationTokenSource();
        private bool _disposed = false;
    }
}