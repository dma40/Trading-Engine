using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class TradingEngine: IMatchingEngine, IDisposable
    {
        protected void ProcessIcebergOrders(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                DateTime now = DateTime.Now;
                TimeSpan currentTime = now.TimeOfDay;

                lock (_ordersLock)
                {
                    if (currentTime >= marketOpen && currentTime <= marketEnd)
                    {
                        var route = _router.Iceberg;
                        var queue = route.queue;

                        foreach (var order in queue)
                        {
                            var iceberg = order.Value;

                            if (iceberg.CurrentQuantity == 0 && !iceberg.isEmpty)
                            {
                                iceberg.replenish();
                                addOrder(iceberg);
                            }

                            else if (iceberg.isEmpty)
                            {
                                queue.Remove(iceberg.OrderID);
                            }
                        }
                    }

                    else
                    {
                        var route = _router.Iceberg;
                        var queue = route.queue;

                        foreach (var order in queue)
                        {
                            Order iceberg = order.Value;

                            if (iceberg.isEmpty)
                            {
                                route.Remove(iceberg.cancelOrder());
                            }
                        }
                    }

                    // add extra cases for debugging this method
                }

                // await Task.Delay(200, token);
            }
        }
    }
}