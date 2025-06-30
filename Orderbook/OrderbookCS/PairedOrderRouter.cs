using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public class PairedOrderRouter
    {
        public PairedOrderRouter()
        {
            routes = new Dictionary<OrderTypes, OrderRoute<PairedOrder>>(); 

            routes.Add(key: OrderTypes.LimitOnOpen, value: new OrderRoute<PairedOrder>());
            routes.Add(key: OrderTypes.MarketOnOpen, value: new OrderRoute<PairedOrder>());
        }

        public void Route(PairedOrder order)
        {
            if (routes.TryGetValue(order.OrderType, out OrderRoute<PairedOrder>? strategy))
            {
                strategy.Route(order);
            }
        }

        public OrderRoute<PairedOrder> PairedCancel => routes[OrderTypes.PairedCancel];
        public OrderRoute<PairedOrder> PairedExecution => routes[OrderTypes.PairedExecution];

        public void Remove(CancelOrder cancel)
        {

        }
        public readonly Dictionary<OrderTypes, OrderRoute<PairedOrder>> routes;
    }

    public class PairedCancelRouter: IPairedRouter
    {
        public PairedCancelRouter()
        {
            queue = new Dictionary<long, PairedOrder>();
        }

        public void Route(PairedOrder paired) => queue.Add(paired.OrderID, paired);
        public void Remove(CancelOrder cancel)
        {
            if (queue.TryGetValue(cancel.OrderID, out PairedOrder? paired))
            {
                queue.Remove(cancel.OrderID);
            }
        }
        public Dictionary<long, PairedOrder> queue { get; private set; }
    }

    public class PairedExecutionRouter : IPairedRouter
    {
        public PairedExecutionRouter()
        {
            queue = new Dictionary<long, PairedOrder>();
        }

        public void Route(PairedOrder paired) => queue.Add(paired.OrderID, paired);
        public void Remove(CancelOrder cancel)
        {
            if (queue.TryGetValue(cancel.OrderID, out PairedOrder? paired))
            {
                queue.Remove(cancel.OrderID);
            }
        }
        public Dictionary<long, PairedOrder> queue { get; private set; }
    }

    public interface IPairedRouter
    {
        public void Route(PairedOrder paired);
        public void Remove(CancelOrder cancel);
        public Dictionary<long, PairedOrder> queue { get; }
    }
}