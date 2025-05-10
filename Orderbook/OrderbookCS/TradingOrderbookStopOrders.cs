using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class TradingOrderbook: OrderEntryOrderbook, ITradingOrderbook, IDisposable
    {
        protected async Task ProcessStopOrders()
        {
            while (true)
            {
                if (_ts.IsCancellationRequested)
                    return;
                
                TimeSpan currentTime = now.TimeOfDay;
                DateTime current = DateTime.Now;
                
                if (currentTime > marketOpen && currentTime < marketEnd)
                {   
                    lock (_stopLock)
                    {
                        foreach (var order in _stop)
                        {
                            var tempOrder = order.Value;

                            if (tempOrder.isBuySide)
                            {
                                if (lastTradedPrice <= order.Value.StopPrice)
                                {
                                    Order activated = order.Value.activate();
                                    match(activated);

                                    _stop.Remove(tempOrder.OrderID);
                                }
                            }

                            else
                            {
                                if (lastTradedPrice >= order.Value.StopPrice)
                                {
                                    Order activated = order.Value.activate();
                                    match(activated);

                                    _stop.Remove(tempOrder.OrderID);
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

                    await Task.Delay(closed);
                }

                if (_ts.IsCancellationRequested)
                    return;

                await Task.Delay(200, _ts.Token);
            }
        }

        protected async Task ProcessTrailingStopOrders()
        {
            while (true)
            {
                if (_ts.IsCancellationRequested)
                    return;

                TimeSpan currentTime = now.TimeOfDay;
                DateTime current = DateTime.Now;

                if (currentTime > marketOpen && currentTime < marketEnd)
                {
                    lock (_stopLock) 
                    {
                        foreach (var trail in _trailingStop)
                        {
                            var trailstop = trail.Value;

                            if (trailstop.isBuySide)
                            {
                                if (lastTradedPrice <= trailstop.StopPrice)
                                {
                                    Order activated = trailstop.activate();
                                    match(activated);

                                    trail.Value.Dispose(); // verify if this is the right behavior
                                    _trailingStop.Remove(trailstop.OrderID);
                                }

                                else if (_greatestTradedPrice > trailstop.currentMaxPrice)
                                    trail.Value.currentMaxPrice = _greatestTradedPrice; 
                            }

                            else 
                            {
                                if (lastTradedPrice >= trailstop.StopPrice)
                                {
                                    Order activated = trailstop.activate();
                                    match(activated);

                                    trail.Value.Dispose(); // verify if this is the correct behavior
                                    _trailingStop.Remove(trailstop.OrderID);
                                }

                                else if (_greatestTradedPrice > trailstop.currentMaxPrice)
                                {
                                    trail.Value.currentMaxPrice = _greatestTradedPrice;
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

                    await Task.Delay(closed);
                }

                if (_ts.IsCancellationRequested) 
                    return;

                await Task.Delay(200, _ts.Token);
            }
        }

        protected async Task ProcessPairedCancelOrders()
        {
            while (true)
            {
                if (_ts.IsCancellationRequested)
                    return;

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

                            if (canMatch(primary) && canMatch(secondary))
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

                                else if (primary.isBuySide)
                                {
                                    
                                }

                                else if (secondary.isBuySide)
                                {

                                }

                                else
                                {

                                }

                                _pairedCancel.Remove(pairedCancelOrder.OrderID);
                            }

                            else if (canMatch(primary))
                            {
                                match(primary);
                                secondary.Dispose();
                                _pairedCancel.Remove(pairedCancelOrder.OrderID);

                            }

                            else if (canMatch(secondary))
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

                    await Task.Delay(closed);
                }

                if (_ts.IsCancellationRequested)
                    return;

                await Task.Delay(200, _ts.Token);
            }
        }

        protected async Task ProcessPairedExecutionOrders()
        {
            while (true)
            {
                if (_ts.IsCancellationRequested)
                    return;

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
                            base.match(activatedPrimary);

                            if (activatedPrimary.CurrentQuantity > 0)
                            {
                                match(activatedPrimary);

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

                    await Task.Delay(closed);
                }

                if (_ts.IsCancellationRequested)
                    return;

                await Task.Delay(200, _ts.Token);
            }
        }
    }
}