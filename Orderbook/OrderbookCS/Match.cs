using System.Text.RegularExpressions;
using TradingServer.Instrument;
using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public class MatchManager: IDisposable
    {
        public MatchManager(Security security)
        {
            Orderbook visible = new Orderbook(security);
            Orderbook hidden = new Orderbook(security);

            orderbook = visible;
            _hidden = hidden;

            _strategies = new Dictionary<OrderTypes, IMatchingStrategy>();

            var MatchAndAddRemaining = new MatchAndAddRemainingStrategy(visible, hidden);
            var ImmediateMatchCancelRemaining = new ImmediateMatchCancelRemainingStrategy(visible, hidden);
            var FillOrKill = new FillOrKillStrategy(visible, hidden);
            var PostOnly = new PostOnlyStrategy(visible, hidden);

            _strategies.Add(key: OrderTypes.Limit, value: MatchAndAddRemaining);
            _strategies.Add(key: OrderTypes.GoodForDay, value: MatchAndAddRemaining);
            _strategies.Add(key: OrderTypes.GoodTillCancel, value: MatchAndAddRemaining);
            _strategies.Add(key: OrderTypes.StopLimit, value: MatchAndAddRemaining);
            _strategies.Add(key: OrderTypes.TrailingStopLimit, value: MatchAndAddRemaining);
            _strategies.Add(key: OrderTypes.Iceberg, value: MatchAndAddRemaining);

            _strategies.Add(key: OrderTypes.Market, value: ImmediateMatchCancelRemaining);
            _strategies.Add(key: OrderTypes.FillAndKill, value: ImmediateMatchCancelRemaining);
            _strategies.Add(key: OrderTypes.StopMarket, value: ImmediateMatchCancelRemaining);
            _strategies.Add(key: OrderTypes.TrailingStopMarket, value: ImmediateMatchCancelRemaining);
            _strategies.Add(key: OrderTypes.MarketOnOpen, value: ImmediateMatchCancelRemaining);
            _strategies.Add(key: OrderTypes.LimitOnOpen, value: ImmediateMatchCancelRemaining);
            _strategies.Add(key: OrderTypes.MarketOnClose, value: ImmediateMatchCancelRemaining);
            _strategies.Add(key: OrderTypes.LimitOnClose, value: ImmediateMatchCancelRemaining);

            _strategies.Add(key: OrderTypes.FillOrKill, FillOrKill);

            _strategies.Add(key: OrderTypes.PostOnly, value: PostOnly);
        }

        public Trades match(Order order)
        {
            Trades result = new Trades();

            if (_strategies.TryGetValue(order.OrderType, out IMatchingStrategy? strategy))
            {
                result = strategy.match(order);
            }

            return result;
        }

        public void ProcessAtMarketEnd()
        {
            orderbook.DeleteExpiredGoodTillCancel();
            _hidden.DeleteExpiredGoodTillCancel();

            orderbook.DeleteGoodForDayOrders();
            _hidden.DeleteGoodForDayOrders();
        }

        public bool containsOrder(Order order)
        {
            return !_hidden.containsOrder(order.OrderID) && !orderbook.containsOrder(order.OrderID);
        }

        public void removeOrder(CancelOrder cancel)
        {
            if (cancel.isHidden)
            {
                _hidden.removeOrder(cancel);
            }

            else
            {
                orderbook.removeOrder(cancel);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~MatchManager()
        {
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool dispose)
        {
            if (_disposed)
            {
                return;
            }

            Interlocked.Exchange(ref _disposed, true);

            if (dispose)
            {
                // Dispose of unmanaged resources, if the need arises.
            }
        }

        private bool _disposed = false;

        public readonly Orderbook orderbook;
        private readonly Orderbook _hidden;

        private readonly Dictionary<OrderTypes, IMatchingStrategy> _strategies;
    }

    public class MatchAndAddRemainingStrategy : IMatchingStrategy
    {
        public MatchAndAddRemainingStrategy(Orderbook _visible, Orderbook _hidden)
        {
            hidden = _hidden;
            visible = _visible;
        }

        public Trades match(Order order)
        {
            Trades result = new Trades();

            result.addTransactions(visible.match(order));
            result.addTransactions(hidden.match(order));

            if (order.CurrentQuantity > 0)
            {
                if (order.isHidden)
                {
                    hidden.addOrder(order);
                }

                else
                {
                    visible.addOrder(order);
                }
            }

            return result;
        }

        private readonly Orderbook hidden;
        private readonly Orderbook visible;
    }

    public class PostOnlyStrategy : IMatchingStrategy
    {
        public PostOnlyStrategy(Orderbook _visible, Orderbook _hidden)
        {
            hidden = _hidden;
            visible = _visible;
        }

        public Trades match(Order order)
        {
            Trades result = new Trades();

            if (!visible.canMatch(order) && !hidden.canMatch(order))
            {
                visible.addOrder(order);
            }

            return result;
        }

        private readonly Orderbook hidden;
        public readonly Orderbook visible;
    }

    public class FillOrKillStrategy : IMatchingStrategy
    {
        public FillOrKillStrategy(Orderbook _visible, Orderbook _hidden)
        {
            hidden = _hidden;
            visible = _visible;
        }

        public Trades match(Order order)
        {
            Trades result = new Trades();

            if (hidden.getEligibleOrderCount(order) + visible.getEligibleOrderCount(order) >= order.Quantity)
            {
                result.addTransactions(visible.match(order));
                result.addTransactions(hidden.match(order));
            }

            return result;
        }

        private readonly Orderbook hidden;
        public readonly Orderbook visible;

    }

    public class ImmediateMatchCancelRemainingStrategy : IMatchingStrategy
    {
        public ImmediateMatchCancelRemainingStrategy(Orderbook _visible, Orderbook _hidden)
        {
            hidden = _hidden;
            visible = _visible;
        }

        public Trades match(Order order)
        {
            Trades result = new Trades();

            result.addTransactions(visible.match(order));
            result.addTransactions(hidden.match(order));

            return result;
        }

        private readonly Orderbook hidden;
        public readonly Orderbook visible;
    }

    public interface IMatchingStrategy
    {
        public Trades match(Order order);
    }
}