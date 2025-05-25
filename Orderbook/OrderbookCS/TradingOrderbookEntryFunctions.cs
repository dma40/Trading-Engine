using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class TradingEngine : IMatchingEngine, IDisposable
    {
        private readonly OrderRouter _router = new OrderRouter();
        private readonly PairedOrderRouter _paired = new PairedOrderRouter();

        public void addOrder(Order order)
        {
            if (DateTime.Now.Hour >= 16 || DateTime.Now.Hour <= 9.5)
            {
                return;
            }

            bool acquired = _semaphore.Wait(TimeSpan.FromSeconds(2), _ts.Token);

            if (acquired)
            {
                if (isQueuable(order))
                {
                    _router.Route(order);
                }

                else
                {
                    if (!orderbook.containsOrder(order.OrderID) && !_hidden.containsOrder(order.OrderID))
                    {
                        //Console.WriteLine("Matching this order");
                        match(order);
                    }

                    else
                    {
                        throw new InvalidOperationException("This order currently exists in the orderbook");
                    }
                }
            }

            _semaphore.Release();
        }

        public async Task addOrder(AbstractPairedOrder paired)
        {
            bool acquired = await _semaphore.WaitAsync(TimeSpan.FromMilliseconds(500), _ts.Token);

            if (acquired)
            {
                _paired.Route(paired);
            }

            _semaphore.Release();
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
                        _router.Route(order);
                        //_iceberg.Add(order.OrderID, order);
                    }
                }

                _semaphore.Release();
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

            bool acquired = _semaphore.Wait(TimeSpan.FromMilliseconds(500), _ts.Token);

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

                _router.Remove(cancel);
                _paired.Remove(cancel);
            }

            _semaphore.Release();
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