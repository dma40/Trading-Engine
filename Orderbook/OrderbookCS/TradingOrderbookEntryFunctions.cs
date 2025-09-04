using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class TradingEngine : IMatchingEngine, IDisposable
    {
        public void addOrder(Order order)
        {
            lock (_ordersLock)
            {
                if (isQueuable(order))
                {
                    _router.Route(order);
                }

                else
                {
                    if (_strategies.containsOrder(order))
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

        public void modifyOrder(ModifyOrder modify)
        {
            lock (_ordersLock)
            {
                removeOrder(modify.cancelOrder());
                addOrder(modify.newOrder());
            }
        }

        public void removeOrder(CancelOrder cancel)
        {
            lock (_ordersLock)
            {
                _strategies.removeOrder(cancel);
                _router.Remove(cancel);
            }
        }

        protected bool isQueuable(Order order)
        {
            return order.OrderType == OrderTypes.MarketOnClose || order.OrderType == OrderTypes.MarketOnOpen
                    || order.OrderType == OrderTypes.LimitOnOpen || order.OrderType == OrderTypes.LimitOnClose;
        }
    }
}