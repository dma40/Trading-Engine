using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class TradingOrderbook: OrderEntryOrderbook, ITradingOrderbook, IDisposable
    {
        private DateTime now;
        private static readonly TimeSpan marketOpen = new TimeSpan(9, 30, 0);
        private static readonly TimeSpan marketEnd = new TimeSpan(16, 0, 0);

        private readonly Dictionary<long, Order> _onMarketOpen = new Dictionary<long, Order>();
        private readonly Dictionary<long, Order> _onMarketClose = new Dictionary<long, Order>();

        protected sealed override async Task ProcessAtMarketEnd()
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
                    now = nextTradingDayStart;

                    lock (_ordersLock)
                    {
                        try
                        {
                            DeleteGoodForDayOrders();
                            DeleteExpiredGoodTillCancel();
                            ProcessOnMarketEndOrders(); 
                        
                            Thread.Sleep(closed);
                        }

                        catch (Exception exception)
                        {
                            Console.WriteLine(exception.Message);
                        }
                    }
                }

                if (_ts.IsCancellationRequested)
                    return; 

                await Task.Delay(200, _ts.Token);
            }
        }

        protected async Task ProcessAtMarketOpen()
        {
            while (true)
            {
                if (_ts.IsCancellationRequested)
                    return;
                
                if (now.TimeOfDay == marketOpen)
                {
                    lock (_ordersLock)
                    {
                        foreach (var order in _onMarketOpen)
                        {
                            var orderEntry = order.Value;
                            var id = order.Value.OrderID;

                            match(order.Value);
                            _onMarketOpen.Remove(id);

                            orderEntry.Dispose();
                        }
                    }
                }

                if (_ts.IsCancellationRequested)
                    return;
                
                await Task.Delay(200, _ts.Token);
            }
        }

        protected void ProcessOnMarketEndOrders()
        {
            foreach (var order in _onMarketClose)
            {
                var current = order.Value;
                match(current);

                _onMarketClose.Remove(current.OrderID);
                current.Dispose();
            }
        }
    }
}