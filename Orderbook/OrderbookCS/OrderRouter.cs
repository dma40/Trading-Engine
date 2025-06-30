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
            routes.Add(key: OrderTypes.TrailingStopMarket, value: new OrderRoute<Order>());
            routes.Add(key: OrderTypes.TrailingStopLimit, value: new OrderRoute<Order>());
            routes.Add(key: OrderTypes.StopMarket, value: new OrderRoute<Order>());
            routes.Add(key: OrderTypes.StopLimit, value: new OrderRoute<Order>());
            routes.Add(key: OrderTypes.Iceberg, value: new OrderRoute<Order>());

            _lockManager = new Dictionary<OrderRoute<Order>, Lock>
            {
                { routes[OrderTypes.LimitOnOpen], new Lock() },
                { routes[OrderTypes.MarketOnOpen], new Lock() },
                { routes[OrderTypes.LimitOnClose], new Lock() },
                { routes[OrderTypes.MarketOnClose], new Lock() },
                { routes[OrderTypes.TrailingStopMarket], new Lock() },
                { routes[OrderTypes.TrailingStopLimit], new Lock() },
                { routes[OrderTypes.StopMarket], new Lock() },
                { routes[OrderTypes.StopLimit], new Lock() },
                { routes[OrderTypes.Iceberg], new Lock() }
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

        public OrderRoute<Order> StopLimit => routes[OrderTypes.StopLimit];
        public OrderRoute<Order> StopMarket => routes[OrderTypes.StopMarket];

        public OrderRoute<Order> TrailingStopLimit => routes[OrderTypes.TrailingStopLimit];
        public OrderRoute<Order> TrailingStopMarket => routes[OrderTypes.TrailingStopMarket];

        public OrderRoute<Order> MarketOnOpen => routes[OrderTypes.MarketOnOpen];
        public OrderRoute<Order> MarketOnClose => routes[OrderTypes.MarketOnClose];

        public OrderRoute<Order> LimitOnOpen => routes[OrderTypes.LimitOnOpen];
        public OrderRoute<Order> LimitOnClose => routes[OrderTypes.LimitOnClose];

        public OrderRoute<Order> Iceberg => routes[OrderTypes.Iceberg];

        private Dictionary<OrderTypes, OrderRoute<Order>> routes;
        private Dictionary<OrderRoute<Order>, Lock> _lockManager;
    }
}