using System.IO.Pipelines;
using System.Runtime.InteropServices;
using TradingServer.Instrument;
using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public class MatchManager
    {
        public MatchManager(Security security)
        {
            Orderbook visible = new Orderbook(security);
            Orderbook hidden = new Orderbook(security);

            orderbook = visible;
            _hidden = hidden;

            _strategies = new Dictionary<OrderTypes, IMatchingStrategy>();

            _strategies.Add(key: OrderTypes.Limit, value: new MatchAndAddRemainingStrategy(visible, hidden));
            _strategies.Add(key: OrderTypes.GoodForDay, value: new MatchAndAddRemainingStrategy(visible, hidden));
            _strategies.Add(key: OrderTypes.GoodTillCancel, value: new MatchAndAddRemainingStrategy(visible, hidden));
            _strategies.Add(key: OrderTypes.StopLimit, value: new MatchAndAddRemainingStrategy(visible, hidden));
            _strategies.Add(key: OrderTypes.TrailingStopLimit, value: new MatchAndAddRemainingStrategy(visible, hidden));
            _strategies.Add(key: OrderTypes.Iceberg, value: new MatchAndAddRemainingStrategy(visible, hidden));

            _strategies.Add(key: OrderTypes.Market, value: new ImmediateMatchCancelRemainingStrategy(visible, hidden));
            _strategies.Add(key: OrderTypes.FillAndKill, value: new ImmediateMatchCancelRemainingStrategy(visible, hidden));
            _strategies.Add(key: OrderTypes.StopMarket, value: new ImmediateMatchCancelRemainingStrategy(visible, hidden));
            _strategies.Add(key: OrderTypes.TrailingStopMarket, value: new ImmediateMatchCancelRemainingStrategy(visible, hidden));
            _strategies.Add(key: OrderTypes.MarketOnOpen, value: new ImmediateMatchCancelRemainingStrategy(visible, hidden));
            _strategies.Add(key: OrderTypes.LimitOnOpen, value: new ImmediateMatchCancelRemainingStrategy(visible, hidden));
            _strategies.Add(key: OrderTypes.MarketOnClose, value: new ImmediateMatchCancelRemainingStrategy(visible, hidden));
            _strategies.Add(key: OrderTypes.LimitOnClose, value: new ImmediateMatchCancelRemainingStrategy(visible, hidden));

            _strategies.Add(key: OrderTypes.FillOrKill, new FillOrKillStrategy(visible, hidden));

            _strategies.Add(key: OrderTypes.PostOnly, value: new PostOnlyStrategy(visible, hidden));
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