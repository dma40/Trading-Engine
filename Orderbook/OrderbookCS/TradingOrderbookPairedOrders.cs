using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class TradingEngine: IMatchingEngine, IDisposable
    {
        private readonly Dictionary<long, PairedCancelOrder> _pairedCancel = new Dictionary<long, PairedCancelOrder>();
        private readonly Dictionary<long, PairedExecutionOrder> _pairedExecution = new Dictionary<long, PairedExecutionOrder>();

        protected async Task ProcessPairedCancelOrders(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                TimeSpan timeOfDay = DateTime.Now.TimeOfDay;
                DateTime current = DateTime.Now;

                if (timeOfDay >= marketOpen && timeOfDay <= marketEnd)
                {
                    lock (_stopLock)
                    {
                        foreach (var pairedCancel in _pairedCancel)
                        {
                            PairedCancelOrder pairedCancelOrder = pairedCancel.Value;
                            Order primary = pairedCancelOrder.primary.activate();
                            Order secondary = pairedCancelOrder.secondary.activate();

                            if (orderbook.canMatch(primary) && orderbook.canMatch(secondary))
                            {
                                if (primary.isBuySide && secondary.isBuySide)
                                {
                                    if (primary.Price > secondary.Price)
                                    {
                                        match(primary);
                                    }

                                    else
                                    {
                                        match(secondary);
                                    }
                                }

                                _pairedCancel.Remove(pairedCancelOrder.OrderID);
                            }

                            else if (orderbook.canMatch(primary))
                            {
                                match(primary);
                                secondary.Dispose();
                                _pairedCancel.Remove(pairedCancelOrder.OrderID);

                            }

                            else if (orderbook.canMatch(secondary))
                            {
                                match(secondary);
                                primary.Dispose();
                                _pairedCancel.Remove(pairedCancelOrder.OrderID);
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

                await Task.Delay(200, token);
            }
        }

        protected async Task ProcessPairedExecutionOrders(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                TimeSpan timeOfDay = DateTime.Now.TimeOfDay;
                DateTime current = DateTime.Now;

                if (timeOfDay > marketOpen && timeOfDay < marketEnd)
                {
                    lock (_stopLock)
                    {
                        foreach (var pairedExecution in _pairedExecution)
                        {
                            PairedExecutionOrder pairedExecutionOrder = pairedExecution.Value;
                            Order primary = pairedExecutionOrder.primary;

                            Order activatedPrimary = primary.activate();
                            match(activatedPrimary);

                            if (activatedPrimary.CurrentQuantity > 0)
                            {
                                Order activatedSecondary = pairedExecutionOrder.secondary.activate();
                                match(activatedSecondary);
                                _pairedExecution.Remove(pairedExecutionOrder.OrderID);
                            }

                            else
                            {
                                match(activatedPrimary);
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

                await Task.Delay(200, token);
            }
        }
    }
}