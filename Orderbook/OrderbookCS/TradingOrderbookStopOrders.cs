using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class TradingEngine: IMatchingEngine, IDisposable
    {
        private readonly Dictionary<long, StopOrder> _stop = new Dictionary<long, StopOrder>();
        private readonly Dictionary<long, TrailingStopOrder> _trailingStop = new Dictionary<long, TrailingStopOrder>();

        protected async Task ProcessStopOrders()
        {
            while (true)
            {
                if (_ts.IsCancellationRequested)
                {
                    return;
                }

                DateTime now = DateTime.Now;
                TimeSpan currentTime = now.TimeOfDay;
                DateTime current = DateTime.Now;

                if (currentTime > marketOpen && currentTime < marketEnd)
                {
                    lock (_stopLock)
                    {
                        foreach (var order in _stop)
                        {
                            StopOrder stop = order.Value;
                            Console.WriteLine("Price of this order: " + stop.StopPrice);
                            Console.WriteLine("Current greatest traded price: " + lastTradedPrice);

                            if (stop.isBuySide)
                            {
                                Console.WriteLine("This is a buy side order. Processing...");
                                if (lastTradedPrice <= stop.StopPrice)
                                {
                                    Console.WriteLine("Activating the buy side order");
                                    Order activated = order.Value.activate();
                                    match(activated);

                                    _stop.Remove(stop.OrderID);
                                }
                            }

                            else
                            {
                                Console.WriteLine("This is a sell side order. Processing...");
                                if (lastTradedPrice >= order.Value.StopPrice)
                                {
                                    Console.WriteLine("Activating the sell side order");
                                    Order activated = order.Value.activate();
                                    match(activated);

                                    _stop.Remove(stop.OrderID);
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
                {
                    return;
                }

                await Task.Delay(200, _ts.Token);
            }
        }
    }
}