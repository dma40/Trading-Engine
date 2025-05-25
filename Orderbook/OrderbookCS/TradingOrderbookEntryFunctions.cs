using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class TradingEngine: IMatchingEngine, IDisposable
    {
        public async Task addOrder(Order order)
        { 
            if (DateTime.Now.Hour >= 16 || DateTime.Now.Hour <= 9.5)
            {
                return;
            }

            bool acquired = await _semaphore.WaitAsync(TimeSpan.FromSeconds(2), _ts.Token);

            if (acquired)
            {
                if (order.OrderType == OrderTypes.LimitOnClose || order.OrderType == OrderTypes.MarketOnClose)
                {
                    if (!_onMarketClose.TryGetValue(order.OrderID, out Order? orderentry))
                    {
                        _onMarketClose.Add(order.OrderID, order);
                    }

                    else
                    {
                        throw new InvalidOperationException("This order already exists in an internal queue");
                    }
                }

                else if (order.OrderType == OrderTypes.LimitOnOpen || order.OrderType == OrderTypes.MarketOnOpen)
                {
                    if (!_onMarketOpen.TryGetValue(order.OrderID, out Order? orderentry))
                    {
                        _onMarketOpen.Add(order.OrderID, order);
                    }

                    else
                    {
                        throw new InvalidOperationException("This order already exists in an internal queue");
                    }
                }

                else 
                {
                    if (!orderbook.containsOrder(order.OrderID) && !_hidden.containsOrder(order.OrderID))
                    {
                        match(order);
                    }

                    else
                    {
                        throw new InvalidOperationException("This order currently exists in the orderbook");
                    }
                }
            }
        }

        public async Task addOrder(StopOrder stop)
        {
            if (DateTime.Now.Hour >= 16 || DateTime.Now.Hour <= 9.5)
            {
                return;
            }

            if (stop.OrderType == OrderTypes.StopLimit || stop.OrderType == OrderTypes.StopMarket)
            {
                bool acquired = await _semaphore.WaitAsync(TimeSpan.FromMilliseconds(500), _ts.Token);

                if (acquired)
                {
                    if (!_stop.TryGetValue(stop.OrderID, out StopOrder? stoporder))
                    {
                        _stop.Add(stop.OrderID, stop);
                    }

                    else
                    {
                        throw new InvalidOperationException("This order already exists in a internal queue");
                    }
                }
            } 
        }

        public async Task addOrder(TrailingStopOrder trail)
        {
            if (DateTime.Now.Hour >= 16 || DateTime.Now.Hour <= 9.5)
            {
                return;
            }

            if (trail.OrderType == OrderTypes.TrailingStopLimit || trail.OrderType == OrderTypes.TrailingStopMarket)
            {
                bool acquired = await _semaphore.WaitAsync(TimeSpan.FromMilliseconds(500), _ts.Token);

                if (acquired)
                {
                    if (!_trailingStop.TryGetValue(trail.OrderID, out TrailingStopOrder? trailstop))
                    {
                        _trailingStop.Add(trail.OrderID, trail);
                    }

                    else
                    {
                        throw new InvalidOperationException("This order already exists in a internal queue");
                    }
                }
            }
        }

        public async Task addOrder(PairedCancelOrder pairedCancel)
        {
            bool acquired = await _semaphore.WaitAsync(TimeSpan.FromMilliseconds(500), _ts.Token);

            if (acquired)
            {
                if (!_pairedCancel.TryGetValue(pairedCancel.OrderID, out PairedCancelOrder? paired))
                {
                    _pairedCancel.Add(pairedCancel.OrderID, pairedCancel);
                }

                else
                {
                    throw new InvalidOperationException("This order already exists in a internal queue");
                }
            }
        }

        public async Task addOrder(IcebergOrder order)
        {
            if (order.OrderType == OrderTypes.Iceberg)
            {
                bool acquired = await _semaphore.WaitAsync(TimeSpan.FromMilliseconds(500), _ts.Token);

                if (acquired)
                {
                    if (!_iceberg.TryGetValue(order.OrderID, out IcebergOrder? _order))
                    {
                        match(order);
                        _iceberg.Add(order.OrderID, order);
                    }
                }
            }
        } 

        public async Task modifyOrder(ModifyOrder modify)
        {
            await removeOrder(modify.cancelOrder());
            await addOrder(modify.newOrder());
        }

        public async Task removeOrder(CancelOrder cancel)
        {
            if (DateTime.Now.Hour >= 16 || DateTime.Now.Hour <= 9.5)
            {
                return;
            }

            bool acquired = await _semaphore.WaitAsync(TimeSpan.FromMilliseconds(500), _ts.Token);

            if (acquired)
            {
                if (cancel.isHidden)
                {
                    _hidden.removeOrder(cancel);
                }

                else
                {
                    orderbook.removeOrder(cancel);
                }

                if (cancel.OrderType == OrderTypes.Iceberg)
                {
                    if (_iceberg.TryGetValue(cancel.OrderID, out IcebergOrder? _order))
                    {
                        if (_order.isEmpty)
                        {
                            _iceberg.Remove(cancel.OrderID);
                        }
                    }
                }

                if (cancel.OrderType == OrderTypes.StopLimit || cancel.OrderType == OrderTypes.StopMarket)
                {
                    if (_stop.TryGetValue(cancel.OrderID, out StopOrder? stop))
                        _stop.Remove(cancel.OrderID);

                    else
                        throw new InvalidOperationException();
                }

                if (cancel.OrderType == OrderTypes.TrailingStopLimit || cancel.OrderType == OrderTypes.TrailingStopMarket)
                {
                    if (_trailingStop.TryGetValue(cancel.OrderID, out TrailingStopOrder? stop))
                        _trailingStop.Remove(cancel.OrderID);

                    else
                        throw new InvalidOperationException();
                }

                if (cancel.OrderType == OrderTypes.LimitOnClose || cancel.OrderType == OrderTypes.MarketOnClose)
                {
                    if (_onMarketClose.TryGetValue(cancel.OrderID, out Order? order))
                    {
                        _onMarketClose.Remove(cancel.OrderID);
                    }

                    else
                    {
                        throw new InvalidOperationException();
                    }
                }

                if (cancel.OrderType == OrderTypes.LimitOnOpen || cancel.OrderType == OrderTypes.MarketOnOpen)
                {
                    if (_onMarketOpen.TryGetValue(cancel.OrderID, out Order? order))
                    {
                        _onMarketOpen.Remove(cancel.OrderID);
                    }

                    else
                    {
                        throw new InvalidOperationException("This order neither exists in the orderbook nor a internal queue");
                    }
                }

                if (cancel.isHidden)
                {
                    _hidden.removeOrder(cancel);
                }

                else
                {
                    orderbook.removeOrder(cancel);
                }
            }
        }

        protected bool isValidTime(IOrderCore order)
        {
            if (order.OrderType == OrderTypes.PostOnly
                || order.OrderType == OrderTypes.Market
                || order.OrderType == OrderTypes.FillOrKill 
                || order.OrderType == OrderTypes.FillAndKill
                || order.OrderType == OrderTypes.StopLimit 
                || order.OrderType == OrderTypes.StopMarket
                || order.OrderType == OrderTypes.TrailingStopLimit
                || order.OrderType == OrderTypes.TrailingStopMarket)
            {
                return DateTime.Now.Hour <= 16 && DateTime.Now.Hour >= 9.5;
            }

            return true;
        }
    }
}