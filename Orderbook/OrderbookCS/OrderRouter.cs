using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public class OrderRouter
    {
        public OrderRouter()
        {
            routes = new Dictionary<OrderTypes, OrderRoute<Order>>();

            routes.Add(key: OrderTypes.LimitOnOpen, value: new OrderRoute<Order>());
            routes.Add(key: OrderTypes.MarketOnOpen, value: new OrderRoute<Order>());
            routes.Add(key: OrderTypes.LimitOnClose, value: new OrderRoute<Order>());
            routes.Add(key: OrderTypes.MarketOnClose, value: new OrderRoute<Order>());

            _lockManager = new Dictionary<OrderRoute<Order>, Lock>
            {
                { routes[OrderTypes.LimitOnOpen], new Lock() },
                { routes[OrderTypes.MarketOnOpen], new Lock() },
                { routes[OrderTypes.LimitOnClose], new Lock() },
                { routes[OrderTypes.MarketOnClose], new Lock() },
            };
        }

        public void Route(Order order)
        {
            if (routes.TryGetValue(order.OrderType, out OrderRoute<Order>? strategy))
            {
                lock (_lockManager[strategy])
                {
                    strategy.Route(order);
                }
            }
        }

        public void Remove(CancelOrder cancel)
        {
            if (routes.TryGetValue(cancel.OrderType, out OrderRoute<Order>? strategy))
            {
                lock (_lockManager[strategy])
                {
                    strategy.Remove(cancel.OrderID);
                }
            }
        }

        public OrderRoute<Order> MarketOnOpen => routes[OrderTypes.MarketOnOpen];
        public OrderRoute<Order> MarketOnClose => routes[OrderTypes.MarketOnClose];

        public OrderRoute<Order> LimitOnOpen => routes[OrderTypes.LimitOnOpen];
        public OrderRoute<Order> LimitOnClose => routes[OrderTypes.LimitOnClose];

        private Dictionary<OrderTypes, OrderRoute<Order>> routes;
        private Dictionary<OrderRoute<Order>, Lock> _lockManager;
    }
}