using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public class OrderRouter
    {
        public OrderRouter()
        {
            routes = new Dictionary<OrderTypes, IRouter>();

            IRouter LimitOnCloseRouter = new OnMarketCloseLimitRouter();
            IRouter MarketOnCloseRouter = new OnMarketCloseMarketRouter();

            IRouter LimitOnOpenRouter = new OnMarketOpenLimitRouter();
            IRouter MarketOnOpenRouter = new OnMarketOpenMarketRouter();

            IRouter TrailingStopLimitRouter = new TrailingStopLimitRouter();
            IRouter TrailingStopMarketRouter = new TrailingStopMarketRouter();

            IRouter StopLimitRouter = new StopLimitRouter();
            IRouter StopMarketRouter = new StopMarketRouter();

            IRouter IcebergRouter = new IcebergRouter();

            routes.Add(key: OrderTypes.LimitOnOpen, value: LimitOnOpenRouter);
            routes.Add(key: OrderTypes.MarketOnOpen, value: MarketOnOpenRouter);
            routes.Add(key: OrderTypes.LimitOnClose, value: LimitOnCloseRouter);
            routes.Add(key: OrderTypes.MarketOnClose, value: MarketOnCloseRouter);
            routes.Add(key: OrderTypes.TrailingStopMarket, value: TrailingStopMarketRouter);
            routes.Add(key: OrderTypes.TrailingStopLimit, value: TrailingStopLimitRouter);
            routes.Add(key: OrderTypes.StopMarket, value: StopMarketRouter);
            routes.Add(key: OrderTypes.StopLimit, value: StopLimitRouter);
            routes.Add(key: OrderTypes.Iceberg, value: IcebergRouter);

            _lockManager = new Dictionary<IRouter, Lock>
            {
                { LimitOnOpenRouter, new Lock() },
                { LimitOnCloseRouter, new Lock() },
                { MarketOnOpenRouter, new Lock() },
                { MarketOnCloseRouter, new Lock() },
                { TrailingStopMarketRouter, new Lock() },
                { TrailingStopLimitRouter, new Lock() },
                { StopLimitRouter, new Lock() },
                { StopMarketRouter, new Lock() },
                { IcebergRouter, new Lock() }
            };
        }

        public void Route(Order order)
        {
            if (routes.TryGetValue(order.OrderType, out IRouter? strategy))
            {
                lock (_lockManager[strategy])
                {
                    strategy.Route(order);
                }
            }
        }

        public void Remove(CancelOrder cancel)
        {
            if (routes.TryGetValue(cancel.OrderType, out IRouter? strategy))
            {
                lock (_lockManager[strategy])
                {
                    strategy.Remove(cancel);
                }
            }
        }

        public IRouter StopLimit => routes[OrderTypes.StopLimit];
        public IRouter StopMarket => routes[OrderTypes.StopMarket];

        public IRouter TrailingStopLimit => routes[OrderTypes.TrailingStopLimit];
        public IRouter TrailingStopMarket => routes[OrderTypes.TrailingStopMarket];

        public IRouter MarketOnOpen => routes[OrderTypes.MarketOnOpen];
        public IRouter MarketOnClose => routes[OrderTypes.MarketOnClose];

        public IRouter LimitOnOpen => routes[OrderTypes.LimitOnOpen];
        public IRouter LimitOnClose => routes[OrderTypes.LimitOnClose];

        public IRouter Iceberg => routes[OrderTypes.Iceberg];

        private Dictionary<OrderTypes, IRouter> routes;
        private Dictionary<IRouter, Lock> _lockManager;
    }

    public class OnMarketCloseMarketRouter : IRouter
    {
        public OnMarketCloseMarketRouter()
        {
            queue = new Dictionary<long, Order>();
        }

        public void Route(Order order) => queue.Add(order.OrderID, order);
        public void Remove(CancelOrder cancel)
        {
            if (queue.TryGetValue(cancel.OrderID, out Order? order) && order != null)
            {
                queue.Remove(cancel.OrderID);
            }
        }
        public Dictionary<long, Order> queue { get; private set; }
    }

    public class OnMarketCloseLimitRouter : IRouter
    {
        public OnMarketCloseLimitRouter()
        {
            queue = new Dictionary<long, Order>();
        }

        public void Route(Order order) => queue.Add(order.OrderID, order);
        public void Remove(CancelOrder cancel)
        {
            if (queue.TryGetValue(cancel.OrderID, out Order? order))
            {
                queue.Remove(cancel.OrderID);
            }
        }
        public Dictionary<long, Order> queue { get; private set; }
    }

    public class OnMarketOpenMarketRouter: IRouter
    {
        public OnMarketOpenMarketRouter()
        {
            queue = new Dictionary<long, Order>();
        }

        public void Route(Order order) => queue.Add(order.OrderID, order);
        public void Remove(CancelOrder cancel)
        {
            if (queue.TryGetValue(cancel.OrderID, out Order? order))
            {
                queue.Remove(cancel.OrderID);
            }
        }
        public Dictionary<long, Order> queue { get; private set; }
    }

    public class OnMarketOpenLimitRouter: IRouter
    {
        public OnMarketOpenLimitRouter()
        {
            queue = new Dictionary<long, Order>();
        }

        public void Route(Order order) => queue.Add(order.OrderID, order);
        public void Remove(CancelOrder cancel)
        {
            if (queue.TryGetValue(cancel.OrderID, out Order? order))
            {
                queue.Remove(cancel.OrderID);
            }
        }
        public Dictionary<long, Order> queue { get; private set; }
    }

    public class IcebergRouter: IRouter
    {
        public IcebergRouter()
        {
            queue = new Dictionary<long, Order>();
        }

        public void Route(Order order) => queue.Add(order.OrderID, order);
        public void Remove(CancelOrder cancel)
        {
            if (queue.TryGetValue(cancel.OrderID, out Order? order))
            {
                queue.Remove(cancel.OrderID);
            }
        }
        public Dictionary<long, Order> queue { get; private set; }
    }   

    public class StopRouter: IRouter
    {
        public StopRouter()
        {
            queue = new Dictionary<long, Order>();
        }

        public void Route(Order order) => queue.Add(order.OrderID, order);
        public void Remove(CancelOrder cancel)
        {
            if (queue.TryGetValue(cancel.OrderID, out Order? order))
            {
                queue.Remove(cancel.OrderID);
            }
        }
        public Dictionary<long, Order> queue { get; private set; }
    }   

    public class TrailingStopLimitRouter: IRouter
    {
        public TrailingStopLimitRouter()
        {
            queue = new Dictionary<long, Order>();
        }

        public void Route(Order order) => queue.Add(order.OrderID, order);
        public void Remove(CancelOrder cancel)
        {
            if (queue.TryGetValue(cancel.OrderID, out Order? order) && order != null)
            {
                queue.Remove(cancel.OrderID);
            }
        }
        public Dictionary<long, Order> queue { get; private set; }
    }   

    public class TrailingStopMarketRouter: IRouter
    {
        public TrailingStopMarketRouter()
        {
            queue = new Dictionary<long, Order>();
        }

        public void Route(Order order) => queue.Add(order.OrderID, order);
        public void Remove(CancelOrder cancel)
        {
            if (queue.TryGetValue(cancel.OrderID, out Order? order))
            {
                queue.Remove(cancel.OrderID);
            }
        }
        public Dictionary<long, Order> queue { get; private set; }
    }  

    public class StopLimitRouter: IRouter
    {
        public StopLimitRouter()
        {
            queue = new Dictionary<long, Order>();
        }

        public void Route(Order order) => queue.Add(order.OrderID, order);
        public void Remove(CancelOrder cancel)
        {
            if (queue.TryGetValue(cancel.OrderID, out Order? order))
            {
                queue.Remove(cancel.OrderID);
            }
        }
        public Dictionary<long, Order> queue { get; private set; }
    }

    public class StopMarketRouter : IRouter
    {
        public StopMarketRouter()
        {
            queue = new Dictionary<long, Order>();
        }

        public void Route(Order order) => queue.Add(order.OrderID, order);
        public void Remove(CancelOrder cancel)
        {
            if (queue.TryGetValue(cancel.OrderID, out Order? order) && order != null)
            {
                queue.Remove(cancel.OrderID);
            }
        }
        public Dictionary<long, Order> queue { get; private set; }
    }

    public interface IRouter
    {
        public void Route(Order order);
        public void Remove(CancelOrder cancel);
        public Dictionary<long, Order> queue { get; }
    }
}