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
            routes.Add(key: OrderTypes.TrailingStopMarket, value: new TrailingStopLimitRouter());
            routes.Add(key: OrderTypes.TrailingStopLimit, value: new TrailingStopLimitRouter());
            routes.Add(key: OrderTypes.StopMarket, value: new StopMarketRouter());
            routes.Add(key: OrderTypes.StopLimit, value: new StopLimitRouter());

            routes.Add(key: OrderTypes.Iceberg, value: new IcebergRouter());
            /* Add remove method as well, simplify remove method*/
        }

        public void Route(Order order)
        {
            if (routes.TryGetValue(order.OrderType, out IRouter? strategy))
            {
                strategy.Route(order); // check if this works okay
            }
        }

        public void Remove(CancelOrder cancel)
        {
            if (routes.TryGetValue(cancel.OrderType, out IRouter? strategy))
            {
                strategy.Remove(cancel);
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
        public void Remove(CancelOrder cancel)
        {
            if (queue.TryGetValue(cancel.OrderID, out Order? order) && order != null)
            {
                queue.Remove(cancel.OrderID);
            }
        }
        public readonly Dictionary<long, Order> queue;
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
        public readonly Dictionary<long, Order> queue;
    }

    public class OnMarketOpenMarketRouter : IRouter
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
        public readonly Dictionary<long, Order> queue;
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
        public readonly Dictionary<long, Order> queue;
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
        public readonly Dictionary<long, Order> queue;
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
        public readonly Dictionary<long, Order> queue;
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
        public readonly Dictionary<long, Order> queue;
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
        public readonly Dictionary<long, Order> queue;
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
        public readonly Dictionary<long, Order> queue;
    } 

    public class StopMarketRouter: IRouter
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
        public readonly Dictionary<long, Order> queue;
    }

    interface IRouter
    {
        public void Route(Order order);
        public void Remove(CancelOrder cancel);
    }
}