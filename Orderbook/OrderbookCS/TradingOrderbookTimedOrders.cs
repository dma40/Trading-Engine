using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class TradingEngine: IMatchingEngine, IDisposable
    {
        private static readonly TimeSpan marketOpen = new TimeSpan(9, 30, 0);
        private static readonly TimeSpan marketEnd = new TimeSpan(16, 0, 0);

        private readonly Dictionary<long, Order> _onMarketOpen = new Dictionary<long, Order>();
        private readonly Dictionary<long, Order> _onMarketClose = new Dictionary<long, Order>();

        protected async Task ProcessAtMarketEnd()
        {
            while (true)
            {
                if (_ts.IsCancellationRequested)
                {
                    return;
                }
                
                DateTime currentTime = DateTime.Now;

                if (currentTime.TimeOfDay == marketEnd)
                {
                    lock (_stopLock)
                    {
                        try
                        {
                            orderbook.DeleteGoodForDayOrders();
                            orderbook.DeleteExpiredGoodTillCancel();

                            _hidden.DeleteGoodForDayOrders();
                            _hidden.DeleteExpiredGoodTillCancel();

                            ProcessOnMarketEndOrders(); 
                        }

                        catch (Exception exception)
                        {
                            Console.WriteLine(exception.Message);
                        }
                    }
                }

                else
                {
                    DateTime tomorrow = currentTime.AddDays(1);
                    DateTime nextTradingDayEnd = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 16, 0, 0);
                    TimeSpan closed = nextTradingDayEnd - DateTime.Now;

                    await Task.Delay(closed, _ts.Token);
                }

                if (_ts.IsCancellationRequested)
                {
                    return;
                }

                await Task.Delay(200, _ts.Token);
            }
        }

        protected async Task ProcessAtMarketOpen()
        {
            while (true)
            {
                if (_ts.IsCancellationRequested)
                {
                    return;
                }

                DateTime currentTime = DateTime.Now;
                DateTime now = DateTime.Now;
                
                if (now.TimeOfDay == marketOpen)
                {
                    lock (_stopLock)
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

                else
                {
                    DateTime tomorrow = currentTime.AddDays(1);
                    DateTime nextTradingDayStart = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 9, 30, 0);
                    TimeSpan closed = nextTradingDayStart - DateTime.Now;

                    await Task.Delay(closed, _ts.Token);
                }

                if (_ts.IsCancellationRequested)
                {
                    return;
                }
                
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