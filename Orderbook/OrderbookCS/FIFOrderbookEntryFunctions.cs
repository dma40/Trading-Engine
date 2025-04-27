using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class TradingOrderbook: OrderEntryOrderbook, ITradingOrderbook, IDisposable
    {
        private readonly Dictionary<long, StopOrder> _stop = new Dictionary<long, StopOrder>();
        private readonly Dictionary<long, TrailingStopOrder> _trailingStop = new Dictionary<long, TrailingStopOrder>();

        public sealed override void addOrder(Order order)
        { 
            if (order.OrderType == OrderTypes.LimitOnClose || order.OrderType == OrderTypes.MarketOnClose)
            {
                if (!_onMarketClose.TryGetValue(order.OrderID, out Order? orderentry) && orderentry != null)
                    _onMarketClose.Add(order.OrderID, order);
                else
                    throw new InvalidOperationException();
            }

            else if (order.OrderType == OrderTypes.LimitOnOpen || order.OrderType == OrderTypes.MarketOnOpen)
            {
                if (!_onMarketOpen.TryGetValue(order.OrderID, out Order? orderentry) && orderentry != null)
                    _onMarketOpen.Add(order.OrderID, order);
                else
                    throw new InvalidOperationException(); 
            }

            else 
            {
                if (!containsOrder(order.OrderID))
                    match(order);
                else
                    throw new InvalidOperationException();
            }
        }

        public void addOrder(StopOrder stop)
        {
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

        public sealed override void modifyOrder(ModifyOrder modify)
        {
            removeOrder(modify.cancelOrder());
            addOrder(modify.newOrder());
        }

        public sealed override void removeOrder(CancelOrder cancel)
        {
            if (cancel.OrderType == OrderTypes.StopLimit || cancel.OrderType == OrderTypes.StopMarket)
            {
                if (_stop.TryGetValue(cancel.OrderID, out StopOrder? stop) && stop != null)
                    _stop.Remove(cancel.OrderID);
                else
                    throw new InvalidOperationException();
            }

            else if (cancel.OrderType == OrderTypes.TrailingStopLimit || cancel.OrderType == OrderTypes.TrailingStopMarket)
            {
                if (_trailingStop.TryGetValue(cancel.OrderID, out TrailingStopOrder? stop) && stop != null)
                    _trailingStop.Remove(cancel.OrderID);
                else
                    throw new InvalidOperationException();
            }

            else if (cancel.OrderType == OrderTypes.LimitOnClose || cancel.OrderType == OrderTypes.MarketOnClose)
            {
                if (_onMarketClose.TryGetValue(cancel.OrderID, out Order? order) && order != null)
                    _onMarketClose.Remove(cancel.OrderID);
                else
                    throw new InvalidOperationException();
            }

            else if (cancel.OrderType == OrderTypes.LimitOnOpen || cancel.OrderType == OrderTypes.MarketOnOpen)
            {
                if (_onMarketOpen.TryGetValue(cancel.OrderID, out Order? order) && order != null)
                    _onMarketOpen.Remove(cancel.OrderID);
                else
                    throw new InvalidOperationException();
            }

            else 
            {
                base.removeOrder(cancel);
            }
        }
    }
}