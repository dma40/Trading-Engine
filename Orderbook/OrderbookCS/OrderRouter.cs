using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public class OrderRouter: IRouter
    {
        public OrderRouter()
        {
            routes = new Dictionary<OrderTypes, IRouter>(); // process the market queue first!

            routes.Add(key: OrderTypes.LimitOnOpen, value: new OnMarketOpenLimitRouter());
            routes.Add(key: OrderTypes.MarketOnOpen, value: new OnMarketOpenMarketRouter());
            routes.Add(key: OrderTypes.LimitOnClose, value: new OnMarketCloseLimitRouter());
            routes.Add(key: OrderTypes.MarketOnClose, value: new OnMarketCloseMarketRouter());
            routes.Add(key: OrderTypes.Iceberg, value: new IcebergRouter());
        }

        public void Route(Order order)
        {
            if (routes.TryGetValue(order.OrderType, out IRouter? strategy))
            {
                strategy.Route(order); // check if this works okay
            }
        }
        private Dictionary<OrderTypes, IRouter> routes;
    }

    public class OnMarketCloseMarketRouter: IRouter
    {
        public OnMarketCloseMarketRouter()
        {
            queue = new Dictionary<long, Order>();
        }

        public void Route(Order order) => queue.Add(order.OrderID, order);
        public readonly Dictionary<long, Order> queue;
    }

    public class OnMarketCloseLimitRouter : IRouter
    {
        public OnMarketCloseLimitRouter()
        {
            queue = new Dictionary<long, Order>();
        }

        public void Route(Order order) => queue.Add(order.OrderID, order);
        public readonly Dictionary<long, Order> queue;
    }

    public class OnMarketOpenMarketRouter : IRouter
    {
        public OnMarketOpenMarketRouter()
        {
            queue = new Dictionary<long, Order>();
        }

        public void Route(Order order) => queue.Add(order.OrderID, order);
        public readonly Dictionary<long, Order> queue;
    }

    public class OnMarketOpenLimitRouter: IRouter
    {
        public OnMarketOpenLimitRouter()
        {
            queue = new Dictionary<long, Order>();
        }

        public void Route(Order order) => queue.Add(order.OrderID, order);
        public readonly Dictionary<long, Order> queue;
    }

    public class IcebergRouter: IRouter
    {
        public IcebergRouter()
        {
            queue = new Dictionary<long, Order>();
        }

        public void Route(Order order) => queue.Add(order.OrderID, order);
        public readonly Dictionary<long, Order> queue;
    }   

   interface IRouter
    {
        public void Route(Order order);
    }
}