using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class TradingEngine: IMatchingEngine, IDisposable
    {
        protected async Task ProcessPairedCancelOrders(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                TimeSpan timeOfDay = DateTime.Now.TimeOfDay;
                DateTime current = DateTime.Now;

                if (timeOfDay >= marketOpen && timeOfDay <= marketEnd)
                {
                    lock (_ordersLock)
                    {
                        var route = _paired.PairedCancel;
                        var paired_queue = route.queue;

                        foreach (var pairedCancel in paired_queue)
                        {
                            AbstractPairedOrder pairedCancelOrder = pairedCancel.Value;
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

                                route.Remove(new CancelOrder(pairedCancelOrder)); // format this better
                            }

                            else if (orderbook.canMatch(primary))
                            {
                                match(primary);
                                secondary.Dispose();
                                route.Remove(new CancelOrder(pairedCancelOrder));

                            }

                            else if (orderbook.canMatch(secondary))
                            {
                                match(secondary);
                                primary.Dispose();
                                route.Remove(new CancelOrder(pairedCancelOrder));
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

                // await Task.Delay(200, token);
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
                    lock (_ordersLock)
                    {
                        var route = _paired.PairedExecution;
                        var paired_queue = route.queue;

                        foreach (var pairedExecution in paired_queue)
                        {
                            AbstractPairedOrder pairedExecutionOrder = pairedExecution.Value;
                            Order primary = pairedExecutionOrder.primary;

                            Order activatedPrimary = primary.activate();
                            match(activatedPrimary);

                            if (activatedPrimary.CurrentQuantity > 0)
                            {
                                Order activatedSecondary = pairedExecutionOrder.secondary.activate();
                                match(activatedSecondary);
                                route.Remove(new CancelOrder(pairedExecutionOrder));
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

                // await Task.Delay(200, token);
            }
        }
    }
}