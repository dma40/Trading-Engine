using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class OrderEntryOrderbook: RetrievalOrderbook, IOrderEntryOrderbook, IDisposable
    {
        private static readonly TimeSpan marketEnd = new TimeSpan(16, 0, 0);

        protected virtual async Task ProcessAtMarketEnd()
        {
            while (true)
            {
                if (_ts.IsCancellationRequested)
                    return;
                
                DateTime currentTime = DateTime.Now;

                if (currentTime.TimeOfDay >= marketEnd)
                {
                    DateTime tomorrow = currentTime.AddDays(1);
                    DateTime nextTradingDayStart = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 9, 30, 0);
                    TimeSpan closed = nextTradingDayStart - DateTime.Now;

                    lock (_ordersLock)
                    {
                        try
                        {
                            DeleteGoodForDayOrders();
                            DeleteExpiredGoodTillCancel();
                        }

                        catch (Exception)
                        {
                            Console.WriteLine("Something went wrong when processing these orders");
                        }                
                    }

                    await Task.Delay(closed);
                }

                await Task.Delay(200, _ts.Token);

                if (_ts.IsCancellationRequested)
                    return;
            }
        }

        protected void DeleteGoodForDayOrders()
        {
            removeOrders(_goodForDay.Values.ToList());
        }

        protected void DeleteExpiredGoodTillCancel()
        {
            List<OrderbookEntry> goodTillCancelOrders = new List<OrderbookEntry>();

            foreach (var order in _goodTillCancel)
            {
                if ((DateTime.UtcNow - order.Value.CreationTime).TotalDays >= 90)
                    goodTillCancelOrders.Add(order.Value);

                removeOrders(goodTillCancelOrders);
            }
        }
    }
}