using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class MatchingOrderbook: Orderbook, IMatchingOrderbook, IDisposable
    {
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

        public long currentPrice()
        {
            return _lastTradedPrice;
        }

    }
}