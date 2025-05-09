using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class TradingOrderbook: OrderEntryOrderbook, ITradingOrderbook, IDisposable
    {
        protected async Task ProcessStopOrders()
        {
            while (true)
            {
                if (_ts.IsCancellationRequested)
                    return;
                
                TimeSpan currentTime = now.TimeOfDay;

                lock (_stopLock)
                {
                    if (currentTime > marketOpen && currentTime < marketEnd)
                    {   
                        foreach (var order in _stop)
                        {
                            var tempOrder = order.Value;

                            if (tempOrder.isBuySide)
                            {
                                if (lastTradedPrice <= order.Value.StopPrice)
                                {
                                    Order activated = order.Value.activate();
                                    match(activated);

                                    _stop.Remove(tempOrder.OrderID);
                                }
                            }

                            else
                            {
                                if (lastTradedPrice >= order.Value.StopPrice)
                                {
                                    Order activated = order.Value.activate();
                                    match(activated);

                                    _stop.Remove(tempOrder.OrderID);
                                }     
                            }
                        }
                    }
                }

                if (_ts.IsCancellationRequested)
                    return;

                await Task.Delay(200, _ts.Token);
            }
        }

        protected async Task ProcessTrailingStopOrders()
        {
            while (true)
            {
                if (_ts.IsCancellationRequested)
                    return;

                TimeSpan currentTime = now.TimeOfDay;

                if (currentTime > marketOpen && currentTime < marketEnd)
                {
                    lock (_stopLock) 
                    {
                        foreach (var trail in _trailingStop)
                        {
                            var trailstop = trail.Value;

                            if (trailstop.isBuySide)
                            {
                                if (lastTradedPrice <= trailstop.StopPrice)
                                {
                                    Order activated = trailstop.activate();
                                    match(activated);

                                    trail.Value.Dispose();
                                    _trailingStop.Remove(trailstop.OrderID);
                                }

                                else if (_greatestTradedPrice > trailstop.currentMaxPrice)
                                    trail.Value.currentMaxPrice = _greatestTradedPrice; 
                            }

                            else 
                            {
                                if (lastTradedPrice >= trailstop.StopPrice)
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
                        }
                    }
                }

                if (_ts.IsCancellationRequested) 
                    return;

                await Task.Delay(200, _ts.Token);
            }
        }
    }
}