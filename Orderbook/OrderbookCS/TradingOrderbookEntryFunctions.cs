using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class TradingEngine : IMatchingEngine, IDisposable
    {
        private readonly OrderRouter _router = new OrderRouter();
        private readonly PairedOrderRouter _paired = new PairedOrderRouter();

        public void addOrder(Order order)
        {
            if (DateTime.Now.Hour >= 16 || DateTime.Now.Hour <= 9.5) // update this method to use isValidTime
            {
                return;
            }

            lock (_ordersLock)
            {
                if (isQueuable(order))
                {
                    _router.Route(order);
                }

                else
                {
                    // Console.WriteLine("This order is not going to go into a queue. ");
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
            if (DateTime.Now.Hour >= 16 || DateTime.Now.Hour <= 9.5)
            {
                return;
            }

            lock (_ordersLock)
            {
                _strategies.removeOrder(cancel);

                _router.Remove(cancel);
                _paired.Remove(cancel);
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

        protected bool isQueuable(Order order)
        {
            return order.OrderType == OrderTypes.MarketOnClose || order.OrderType == OrderTypes.MarketOnOpen
                    || order.OrderType == OrderTypes.LimitOnOpen || order.OrderType == OrderTypes.LimitOnClose
                    || order.OrderType == OrderTypes.PairedCancel || order.OrderType == OrderTypes.PairedExecution;
        }
    }
}