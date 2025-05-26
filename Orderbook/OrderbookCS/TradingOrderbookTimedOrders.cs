using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class TradingEngine: IMatchingEngine, IDisposable
    {
        private static readonly TimeSpan marketOpen = new TimeSpan(9, 30, 0);
        private static readonly TimeSpan marketEnd = new TimeSpan(16, 0, 0);

        protected async Task ProcessAtMarketEnd(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {    
                DateTime currentTime = DateTime.Now;

                if (currentTime.TimeOfDay == marketEnd)
                {
                    lock (_ordersLock)
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

                    await Task.Delay(closed, token);
                }

                await Task.Delay(200, token);
            }
        }

        protected async Task ProcessAtMarketOpen(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                DateTime currentTime = DateTime.Now;
                DateTime now = DateTime.Now;

                if (now.TimeOfDay == marketOpen)
                {
                    var route = _router.MarketOnOpen;
                    var market_queue = route.queue;

                    lock (_ordersLock)
                    {
                        foreach (var order in market_queue)
                        {
                            var orderEntry = order.Value;

                            match(orderEntry);
                        }

                        foreach (var order in market_queue)
                        {
                            var orderEntry = order.Value;

                            route.Remove(orderEntry.cancelOrder());
                        }

                        var limitRoute = _router.LimitOnOpen;
                        var limit_queue = limitRoute.queue;

                        foreach (var order in limit_queue)
                        {
                            var orderEntry = order.Value;

                            match(orderEntry);
                        }

                        foreach (var order in market_queue)
                        {
                            var orderEntry = order.Value;

                            route.Remove(orderEntry.cancelOrder());
                        }
                    }
                }

                else
                {
                    DateTime tomorrow = currentTime.AddDays(1);
                    DateTime nextTradingDayStart = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 9, 30, 0);
                    TimeSpan closed = nextTradingDayStart - DateTime.Now;

                    await Task.Delay(closed, token);
                }
                
                //await Task.Delay(200, token);
            }
        }

        protected void ProcessOnMarketEndOrders()
        {
            var route = _router.MarketOnClose;
            var market_queue = route.queue;

            foreach (var order in market_queue)
            {
                var orderEntry = order.Value;

                match(orderEntry);
            }

            foreach (var order in market_queue)
            {
                var orderEntry = order.Value;

                route.Remove(orderEntry.cancelOrder());
            }

            var limitRoute = _router.LimitOnOpen;
            var limit_queue = limitRoute.queue;

            foreach (var order in limit_queue)
            {
                var orderEntry = order.Value;

                match(orderEntry);
            }

            foreach (var order in limit_queue)
            {
                var orderEntry = order.Value;

                route.Remove(orderEntry.cancelOrder());
            }
        }
    }
}