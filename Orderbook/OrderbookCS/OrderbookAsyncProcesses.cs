using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class Orderbook: IRetrievalOrderbook, IDisposable
    {
        private readonly Mutex _orderMutex = new Mutex();
        private readonly Mutex _goodForDayMutex = new Mutex();
        private readonly Mutex _goodTillCancelMutex = new Mutex();

        private DateTime now;
        private static readonly TimeSpan marketOpen = new TimeSpan(9, 30, 0);
        private static readonly TimeSpan marketEnd = new TimeSpan(16, 0, 0);
        
        private Trades _trades;
        private long _greatestTradedPrice = Int32.MinValue;
        private long _lastTradedPrice;

        private readonly Dictionary<long, StopOrder> _stop = new Dictionary<long, StopOrder>();
        private readonly Dictionary<long, TrailingStopOrder> _trailingStop = new Dictionary<long, TrailingStopOrder>();

        protected async Task ProcessAtMarketEnd()
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
            // must be in before 9:28 AM local time (or throw a error otherwise)
        }

        protected void ProcessOnMarketEndOrders()
        {
            foreach (var order in _onMarketClose)
            {
                var current = order.Value;
                match(current.CurrentOrder);

                _onMarketClose.Remove(current.CurrentOrder.OrderID);
                current.Dispose();
            }
            // that they need to be placed before 3:50 PM or throw a error otherwise
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
                            if (_lastTradedPrice >= order.Value.StopPrice)
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

        protected void DeleteExpiredGoodTillCancel()
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

                await Task.Delay(200, _ts.Token);
            }
        }

    }
}