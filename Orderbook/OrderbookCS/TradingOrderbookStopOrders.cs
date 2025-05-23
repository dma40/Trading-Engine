using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class TradingEngine: IMatchingEngine, IDisposable
    {
        protected async Task ProcessStopOrders()
        {
            while (true)
            {
                if (_ts.IsCancellationRequested)
                    return;
                
                DateTime now = DateTime.Now;
                TimeSpan currentTime = now.TimeOfDay;
                DateTime current = DateTime.Now;
                
                if (currentTime > marketOpen && currentTime < marketEnd)
                {   
                    lock (_stopLock)
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

                else
                {
                    DateTime tomorrow = current.AddDays(1);
                    DateTime nextTradingDayStart = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 9, 30, 0);
                    TimeSpan closed = nextTradingDayStart - DateTime.Now;

                    await Task.Delay(closed);
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

                DateTime now = DateTime.Now;
                TimeSpan currentTime = now.TimeOfDay;
                DateTime current = DateTime.Now;

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

                                    _trailingStop.Remove(trailstop.OrderID);
                                }

                                else if (greatestTradedPrice > trailstop.currentMaxPrice)
                                    trail.Value.currentMaxPrice = greatestTradedPrice; 
                            }

                            else 
                            {
                                if (lastTradedPrice >= trailstop.StopPrice)
                                {
                                    Order activated = trailstop.activate();
                                    match(activated);

                                    _trailingStop.Remove(trailstop.OrderID);
                                }

                                else if (greatestTradedPrice > trailstop.currentMaxPrice)
                                {
                                    trail.Value.currentMaxPrice = greatestTradedPrice;
                                }
                            }
                        }
                    }
                }

                else
                {
                    DateTime tomorrow = current.AddDays(1);
                    DateTime nextTradingDayStart = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 9, 30, 0);
                    TimeSpan closed = nextTradingDayStart - DateTime.Now;

                    await Task.Delay(closed);
                }

                if (_ts.IsCancellationRequested) 
                    return;

                await Task.Delay(200, _ts.Token);
            }
        }
    }
}