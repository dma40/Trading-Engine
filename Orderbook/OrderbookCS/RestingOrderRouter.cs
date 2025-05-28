using TradingServer.OrderbookCS;

namespace TradingServer.Orders
{
    public class RestingRouter
    {
        public RestingRouter()
        {
            _routes = new Dictionary<OrderTypes, IRestingRouter>();

            _routes.Add(key: OrderTypes.GoodForDay, value: new GoodForDayRouter());
            _routes.Add(key: OrderTypes.GoodTillCancel, value: new GoodTillCancelRouter());
        }

        public void Route(OrderbookEntry orderbookEntry)
        {
            if (_routes.TryGetValue(orderbookEntry.OrderType, out IRestingRouter? resting))
            {
                resting.Route(orderbookEntry);
            }
        }

        public void Remove(OrderbookEntry orderbookEntry)
        {
            if (_routes.TryGetValue(orderbookEntry.OrderType, out IRestingRouter? resting))
            {
                resting.Remove(orderbookEntry);
            }
        }

        public IRestingRouter goodForDay => _routes[OrderTypes.GoodForDay];
        public IRestingRouter goodTillCancel => _routes[OrderTypes.GoodTillCancel];

        private readonly Dictionary<OrderTypes, IRestingRouter> _routes;
    }

    public class GoodTillCancelRouter : IRestingRouter
    {
        public GoodTillCancelRouter()
        {
            queue = new Dictionary<long, OrderbookEntry>();
        }

        public void Route(OrderbookEntry orderbookEntry)
        {
            if (!queue.TryGetValue(orderbookEntry.OrderID, out OrderbookEntry? obe))
            {
                queue.Add(orderbookEntry.OrderID, orderbookEntry);
            }
        }

        public void Remove(OrderbookEntry orderbookEntry)
        {
            if (queue.TryGetValue(orderbookEntry.OrderID, out OrderbookEntry? obe))
            {
                queue.Remove(obe.OrderID);
            }
        }

        public Dictionary<long, OrderbookEntry> queue { get; private set; }
    }    

    public class GoodForDayRouter : IRestingRouter
    {
        public GoodForDayRouter()
        {
            queue = new Dictionary<long, OrderbookEntry>();
        }

        public void Route(OrderbookEntry orderbookEntry)
        {
            if (!queue.TryGetValue(orderbookEntry.OrderID, out OrderbookEntry? obe))
            {
                queue.Add(orderbookEntry.OrderID, orderbookEntry);
            }
        }

        public void Remove(OrderbookEntry orderbookEntry)
        {
            if (queue.TryGetValue(orderbookEntry.OrderID, out OrderbookEntry? obe))
            {
                queue.Remove(obe.OrderID);
            }
        }

        public Dictionary<long, OrderbookEntry> queue { get; private set; }
    }

    public interface IRestingRouter
    {
        void Route(OrderbookEntry entry);
        void Remove(OrderbookEntry entry);
        Dictionary<long, OrderbookEntry> queue { get; }
    }
}