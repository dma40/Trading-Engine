using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class Orderbook: IRetrievalOrderbook, IDisposable
    {
        private readonly Mutex _orderMutex = new Mutex();
        private readonly Mutex _goodForDayMutex = new Mutex();
        private readonly Mutex _goodTillCancelMutex = new Mutex();

        private static readonly TimeSpan marketEnd = new TimeSpan(16, 0, 0);


        protected virtual async Task ProcessAtMarketEnd()
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
    }
}