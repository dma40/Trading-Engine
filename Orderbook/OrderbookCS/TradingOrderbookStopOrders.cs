using System.ComponentModel;
using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class TradingEngine: IMatchingEngine, IDisposable
    {
        protected async Task ProcessStopOrders(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                DateTime now = DateTime.Now;
                TimeSpan currentTime = now.TimeOfDay;
                DateTime current = DateTime.Now;

                if (currentTime > marketOpen && currentTime < marketEnd)
                {
                    lock (_ordersLock)
                    {
                        var route = _router.StopMarket;
                        var market_queue = route.queue;
                        // make a list of CancelOrders, and then remove all of the orders; 
                        // this method was implemented in Orderbook.
                        foreach (var order in market_queue)
                        {
                            var stop = order.Value;
                            //Console.WriteLine("Price of this order: " + stop.StopPrice);
                            //Console.WriteLine("Current greatest traded price: " + lastTradedPrice);

                            if (stop.isBuySide)
                            {
                                //Console.WriteLine("This is a buy side order. Processing...");
                                if (lastTradedPrice <= stop.StopPrice)
                                {
                                    //Console.WriteLine("Activating the buy side order");
                                    Order activated = order.Value.activate();
                                    match(activated);

                                    route.Remove(stop.cancelOrder()); // do this outside of the method
                                }
                            }

                            else
                            {
                                //Console.WriteLine("This is a sell side order. Processing...");
                                if (lastTradedPrice >= stop.StopPrice)
                                {
                                    //Console.WriteLine("Activating the sell side order");
                                    Order activated = stop.activate();
                                    match(activated);

                                    route.Remove(stop.cancelOrder()); // update this too
                                }
                            }
                        }

                        var limit_route = _router.StopLimit;
                        var limit_queue = limit_route.queue;

                        foreach (var order in limit_queue)
                        {
                            var stop = order.Value;
                            //Console.WriteLine("Price of this order: " + stop.StopPrice);
                            //Console.WriteLine("Current greatest traded price: " + lastTradedPrice);

                            if (stop.isBuySide)
                            {
                                //Console.WriteLine("This is a buy side order. Processing...");
                                if (lastTradedPrice <= stop.StopPrice)
                                {
                                    //Console.WriteLine("Activating the buy side order");
                                    Order activated = order.Value.activate();
                                    match(activated);

                                    route.Remove(stop.cancelOrder()); // do this outside of the method
                                }
                            }

                            else
                            {
                                //Console.WriteLine("This is a sell side order. Processing...");
                                if (lastTradedPrice >= stop.StopPrice)
                                {
                                    //Console.WriteLine("Activating the sell side order");
                                    Order activated = stop.activate();
                                    match(activated);

                                    route.Remove(stop.cancelOrder());
                                }
                            }
                        }
                    }

                   // _semaphore.Release();
                }

                else
                {
                    DateTime tomorrow = current.AddDays(1);
                    DateTime nextTradingDayStart = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 9, 30, 0);
                    TimeSpan closed = nextTradingDayStart - DateTime.Now;

                    await Task.Delay(closed, token);
                }

                // await Task.Delay(200, token);
            }
        }

        protected async Task ProcessTrailingStopOrders(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                DateTime now = DateTime.Now;
                TimeSpan currentTime = now.TimeOfDay;
                DateTime current = DateTime.Now;

                if (currentTime > marketOpen && currentTime < marketEnd)
                {
                    lock (_ordersLock)
                    {
                        var route = _router.TrailingStopMarket;
                        var market_queue = route.queue;

                        foreach (var trail in market_queue)
                        {
                            var trailstop = trail.Value;

                            if (trailstop.isBuySide)
                            {
                                if (lastTradedPrice <= trailstop.StopPrice)
                                {
                                    Order activated = trailstop.activate();
                                    match(activated);

                                    route.Remove(trailstop.cancelOrder()); // may add a few milli/nanoseconds; update this
                                }

                                else if (greatestTradedPrice > trailstop.currentMaxPrice)
                                {
                                    trail.Value.currentMaxPrice = greatestTradedPrice;
                                }
                            }

                            else
                            {
                                if (lastTradedPrice >= trailstop.StopPrice)
                                {
                                    Order activated = trailstop.activate();
                                    match(activated);

                                    route.Remove(trailstop.cancelOrder());
                                }

                                else if (greatestTradedPrice > trailstop.currentMaxPrice)
                                {
                                    trail.Value.currentMaxPrice = greatestTradedPrice;
                                }
                            }
                        }

                        var limit_route = _router.TrailingStopLimit;
                        var limit_queue = limit_route.queue;

                        foreach (var trail in limit_queue)
                        {
                            var trailstop = trail.Value;

                            if (trailstop.isBuySide)
                            {
                                if (lastTradedPrice <= trailstop.StopPrice)
                                {
                                    Order activated = trailstop.activate();
                                    match(activated);

                                    route.Remove(trailstop.cancelOrder()); // may add a few milli/nanoseconds; update this
                                }

                                else if (greatestTradedPrice > trailstop.currentMaxPrice)
                                {
                                    trail.Value.currentMaxPrice = greatestTradedPrice;
                                }
                            }

                            else
                            {
                                if (lastTradedPrice >= trailstop.StopPrice)
                                {
                                    Order activated = trailstop.activate();
                                    match(activated);

                                    route.Remove(trailstop.cancelOrder());
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

                    await Task.Delay(closed, token);
                }

                //await Task.Delay(200, token);
            }
        }
    }
}