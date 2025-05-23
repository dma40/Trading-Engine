namespace TradingServer.OrderbookCS
{
    public partial class TradingEngine: IMatchingEngine, IDisposable
    {
        protected async Task ProcessIcebergOrders() 
        {
            while (true)
            {
                DateTime now = DateTime.Now;
                TimeSpan currentTime = now.TimeOfDay;

                lock (_stopLock)
                {
                    if (currentTime >= marketOpen && currentTime <= marketEnd)
                    {
                        foreach (var order in _iceberg)
                        {
                            var iceberg = order.Value;
                            
                            if (iceberg.CurrentQuantity == 0 && !iceberg.isEmpty)
                            {
                                iceberg.replenish();
                                addOrder(iceberg);
                            }

                            else if (iceberg.isEmpty)
                            {
                                _iceberg.Remove(iceberg.OrderID);
                            }
                        }
                    }

                    else 
                    {
                        foreach (var order in _iceberg)
                        {
                            var iceberg = order.Value;

                            if (iceberg.isEmpty)
                            {
                                _iceberg.Remove(iceberg.OrderID);
                            }
                        }
                    }
                }

                await Task.Delay(200, _ts.Token);
            }
        }
    }
}