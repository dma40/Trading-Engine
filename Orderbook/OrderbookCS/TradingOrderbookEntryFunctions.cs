using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class TradingEngine: IMatchingEngine, IDisposable
    {
        private readonly Dictionary<long, StopOrder> _stop = new Dictionary<long, StopOrder>();
        private readonly Dictionary<long, TrailingStopOrder> _trailingStop = new Dictionary<long, TrailingStopOrder>();
        private readonly Dictionary<long, PairedCancelOrder> _pairedCancel = new Dictionary<long, PairedCancelOrder>();
        private readonly Dictionary<long, PairedExecutionOrder> _pairedExecution = new Dictionary<long, PairedExecutionOrder>();
        private readonly Dictionary<long, IcebergOrder> _iceberg = new Dictionary<long, IcebergOrder>();

        public void addOrder(Order order)
        { 
            /*
            if (DateTime.Now.Hour >= 16 || DateTime.Now.Hour <= 9.5)
            {
                return;
            }
            */

            lock (_stopLock)
            {
                if (order.OrderType == OrderTypes.LimitOnClose || order.OrderType == OrderTypes.MarketOnClose)
                {
                    if (!_onMarketClose.TryGetValue(order.OrderID, out Order? orderentry))
                        _onMarketClose.Add(order.OrderID, order);

                    else
                        throw new InvalidOperationException();
                }

                else if (order.OrderType == OrderTypes.LimitOnOpen || order.OrderType == OrderTypes.MarketOnOpen)
                {
                    if (!_onMarketOpen.TryGetValue(order.OrderID, out Order? orderentry))
                        _onMarketOpen.Add(order.OrderID, order);

                    else
                        throw new InvalidOperationException(); 
                }

                else 
                {
                    if (!orderbook.containsOrder(order.OrderID))
                        match(order);

                    else
                        throw new InvalidOperationException();
                }
            }
        }

        public void addOrder(StopOrder stop)
        {
            if (DateTime.Now.Hour >= 16 || DateTime.Now.Hour <= 9.5)
            {
                return;
            }

            if (stop.OrderType == OrderTypes.StopLimit || stop.OrderType == OrderTypes.StopMarket)
            {
                lock (_stopLock)
                {
                    if (!_stop.TryGetValue(stop.OrderID, out StopOrder? stoporder) && stoporder != null)
                        _stop.Add(stop.OrderID, stop);

                    else
                        throw new InvalidOperationException();
                }
            } 
        }

        public void addOrder(TrailingStopOrder trail)
        {
            if (DateTime.Now.Hour >= 16 || DateTime.Now.Hour <= 9.5)
            {
                return;
            }

            if (trail.OrderType == OrderTypes.TrailingStopLimit || trail.OrderType == OrderTypes.TrailingStopMarket)
            {
                lock (_stopLock)
                {
                    if (!_trailingStop.TryGetValue(trail.OrderID, out TrailingStopOrder? trailstop) && trailstop != null)
                        _trailingStop.Add(trail.OrderID, trail);

                    else
                        throw new InvalidOperationException();
                }
            }
        }

        public void addOrder(PairedCancelOrder pairedCancel)
        {
            lock (_stopLock)
            {
                if (!_pairedCancel.TryGetValue(pairedCancel.OrderID, out PairedCancelOrder? paired))
                    _pairedCancel.Add(pairedCancel.OrderID, pairedCancel);
                
                else
                    throw new InvalidOperationException();
            }
        }

        public void addOrder(IcebergOrder order)
        {
            if (order.OrderType == OrderTypes.Iceberg)
            {
                if (!_iceberg.TryGetValue(order.OrderID, out IcebergOrder? _order))
                {
                    match(order);
                    _iceberg.Add(order.OrderID, order);
                }
            }
        } 

        public void modifyOrder(ModifyOrder modify)
        {
            removeOrder(modify.cancelOrder());
            addOrder(modify.newOrder());
        }

        public void removeOrder(CancelOrder cancel)
        {
            if (DateTime.Now.Hour >= 16 || DateTime.Now.Hour <= 9.5)
            {
                return;
            }

            lock (_stopLock)
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
                    if (_stop.TryGetValue(cancel.OrderID, out StopOrder? stop) && stop != null)
                        _stop.Remove(cancel.OrderID);

                    else
                        throw new InvalidOperationException();
                }

                if (cancel.OrderType == OrderTypes.TrailingStopLimit || cancel.OrderType == OrderTypes.TrailingStopMarket)
                {
                    if (_trailingStop.TryGetValue(cancel.OrderID, out TrailingStopOrder? stop) && stop != null)
                        _trailingStop.Remove(cancel.OrderID);

                    else
                        throw new InvalidOperationException();
                }

                if (cancel.OrderType == OrderTypes.LimitOnClose || cancel.OrderType == OrderTypes.MarketOnClose)
                {
                    if (_onMarketClose.TryGetValue(cancel.OrderID, out Order? order) && order != null)
                        _onMarketClose.Remove(cancel.OrderID);

                    else
                        throw new InvalidOperationException();
                }

                if (cancel.OrderType == OrderTypes.LimitOnOpen || cancel.OrderType == OrderTypes.MarketOnOpen)
                {
                    if (_onMarketOpen.TryGetValue(cancel.OrderID, out Order? order) && order != null)
                        _onMarketOpen.Remove(cancel.OrderID);
                    
                    else
                        throw new InvalidOperationException();
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