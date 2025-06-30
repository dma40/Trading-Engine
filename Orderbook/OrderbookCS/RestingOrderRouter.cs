using TradingServer.OrderbookCS;

namespace TradingServer.Orders
{
    public class RestingRouter
    {
        public RestingRouter()
        {
            _routes = new Dictionary<OrderTypes, OrderRoute<OrderbookEntry>>();

            _routes.Add(key: OrderTypes.GoodForDay, value: new OrderRoute<OrderbookEntry>());
            _routes.Add(key: OrderTypes.GoodTillCancel, value: new OrderRoute<OrderbookEntry>());
        }

        public void Route(OrderbookEntry orderbookEntry)
        {
            if (_routes.TryGetValue(orderbookEntry.OrderType, out OrderRoute<OrderbookEntry>? resting))
            {
                resting.Route(orderbookEntry);
            }
        }

        public void Remove(OrderbookEntry orderbookEntry)
        {
            if (_routes.TryGetValue(orderbookEntry.OrderType, out OrderRoute<OrderbookEntry>? resting))
            {
                resting.Remove(orderbookEntry);
            }
        }

        public OrderRoute<OrderbookEntry> goodForDay => _routes[OrderTypes.GoodForDay];
        public OrderRoute<OrderbookEntry> goodTillCancel => _routes[OrderTypes.GoodTillCancel];

        private readonly Dictionary<OrderTypes, OrderRoute<OrderbookEntry>> _routes;
    }

    public class OrderRoute<T> where T: IOrderCore
    {
        public OrderRoute() {
            queue = new Dictionary<long, T>();
        }

        public void Route(T orderbookEntry)
        {
            if (!queue.TryGetValue(orderbookEntry.OrderID, out T? obe))
            {
                queue.Add(orderbookEntry.OrderID, orderbookEntry);
            }
        }

        public void Remove(T orderbookEntry)
        {
            if (queue.TryGetValue(orderbookEntry.OrderID, out T? obe))
            {
                queue.Remove(orderbookEntry.OrderID);
            }
        }

        public void Remove(CancelOrder cancel)
        {
            if (queue.TryGetValue(cancel.OrderID, out T? obe))
            {
                queue.Remove(cancel.OrderID);
            }
        }

        public void Remove(long id)
        {
            if (queue.TryGetValue(id, out T? t))
            {
                queue.Remove(id);
            }
        }

        public Dictionary<long, T> queue { get; private set; }
    }

    public interface IRestingRouter
    {
        void Route(OrderbookEntry entry);
        void Remove(OrderbookEntry entry);
        Dictionary<long, OrderbookEntry> queue { get; }
    }
}