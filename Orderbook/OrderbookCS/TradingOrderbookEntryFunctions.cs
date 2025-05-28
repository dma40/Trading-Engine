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
                    return;
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

        public void addOrder(AbstractPairedOrder paired)
        {
            lock (_ordersLock)
            {
                _paired.Route(paired);
            }
        }

        public void addOrder(IcebergOrder order)
        {
            if (order.OrderType == OrderTypes.Iceberg)
            {
                lock (_ordersLock)
                {
                    var route = _router.Iceberg;
                    var queue = route.queue;

                    if (!queue.TryGetValue(order.OrderID, out Order? _order))
                    {
                        match(order);
                        _router.Route(order);
                    }
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
            lock (_ordersLock)
            {
                _strategies.removeOrder(cancel);

                _router.Remove(cancel);
                _paired.Remove(cancel);
            }
        }

        protected bool isQueuable(Order order)
        {
            return order.OrderType == OrderTypes.MarketOnClose || order.OrderType == OrderTypes.MarketOnOpen
                    || order.OrderType == OrderTypes.LimitOnOpen || order.OrderType == OrderTypes.LimitOnClose
                    || order.OrderType == OrderTypes.PairedCancel || order.OrderType == OrderTypes.PairedExecution;
        }
    }
}