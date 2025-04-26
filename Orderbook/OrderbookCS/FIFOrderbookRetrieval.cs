using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class MatchingOrderbook: Orderbook, IMatchingOrderbook, IDisposable
    {
        private readonly Dictionary<long, StopOrder> _stop = new Dictionary<long, StopOrder>();
        private readonly Dictionary<long, TrailingStopOrder> _trailingStop = new Dictionary<long, TrailingStopOrder>();

        public sealed override void addOrder(Order order)
        { 
            if (order.OrderType == OrderTypes.StopLimit || order.OrderType == OrderTypes.StopMarket)
            {
                lock (_stopLock)
                {
                    _stop.Add(order.OrderID, (StopOrder) order);
                }
            }

            else if (order.OrderType == OrderTypes.TrailingStop)
            {
                lock (_stopLock)
                {
                    _trailingStop.Add(order.OrderID, (TrailingStopOrder) order);
                }
            }

            else if (order.OrderType == OrderTypes.LimitOnClose || order.OrderType == OrderTypes.MarketOnClose)
            {
                _onMarketClose.Add(order.OrderID, order);
            }

            else if (order.OrderType == OrderTypes.LimitOnOpen || order.OrderType == OrderTypes.MarketOnOpen)
            {
                _onMarketOpen.Add(order.OrderID, order);
            }

            else 
            {
                match(order);
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
                _stop.Remove(cancel.OrderID);
            }

            else if (cancel.OrderType == OrderTypes.TrailingStop)
            {
                _trailingStop.Remove(cancel.OrderID);
            }

            else if (cancel.OrderType == OrderTypes.LimitOnClose || cancel.OrderType == OrderTypes.MarketOnClose)
            {
                _onMarketClose.Remove(cancel.OrderID);
            }

            else if (cancel.OrderType == OrderTypes.LimitOnOpen || cancel.OrderType == OrderTypes.MarketOnOpen)
            {
                _onMarketOpen.Remove(cancel.OrderID);
            }

            else 
            {
                base.removeOrder(cancel);
            }
        }
    }
}