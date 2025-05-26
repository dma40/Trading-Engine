using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public class PairedOrderRouter
    {
        public PairedOrderRouter()
        {
            routes = new Dictionary<OrderTypes, IPairedRouter>(); 

            routes.Add(key: OrderTypes.LimitOnOpen, value: new PairedExecutionRouter());
            routes.Add(key: OrderTypes.MarketOnOpen, value: new PairedCancelRouter());
        }

        public void Route(AbstractPairedOrder order)
        {
            if (routes.TryGetValue(order.OrderType, out IPairedRouter? strategy))
            {
                strategy.Route(order);
            }
        }

        public IPairedRouter PairedCancel => routes[OrderTypes.PairedCancel];
        public IPairedRouter PairedExecution => routes[OrderTypes.PairedExecution];

        public void Remove(CancelOrder cancel)
        {

        }
        public readonly Dictionary<OrderTypes, IPairedRouter> routes;
    }

    public class PairedCancelRouter: IPairedRouter
    {
        public PairedCancelRouter()
        {
            queue = new Dictionary<long, AbstractPairedOrder>();
        }

        public void Route(AbstractPairedOrder paired) => queue.Add(paired.OrderID, paired);
        public void Remove(CancelOrder cancel)
        {
            if (queue.TryGetValue(cancel.OrderID, out AbstractPairedOrder? paired))
            {
                queue.Remove(cancel.OrderID);
            }
        }
        public Dictionary<long, AbstractPairedOrder> queue { get; private set; }
    }

    public class PairedExecutionRouter : IPairedRouter
    {
        public PairedExecutionRouter()
        {
            queue = new Dictionary<long, AbstractPairedOrder>();
        }

        public void Route(AbstractPairedOrder paired) => queue.Add(paired.OrderID, paired);
        public void Remove(CancelOrder cancel)
        {
            if (queue.TryGetValue(cancel.OrderID, out AbstractPairedOrder? paired))
            {
                queue.Remove(cancel.OrderID);
            }
        }
        public Dictionary<long, AbstractPairedOrder> queue { get; private set; }
    }

    public interface IPairedRouter
    {
        public void Route(AbstractPairedOrder paired);
        public void Remove(CancelOrder cancel);
        public Dictionary<long, AbstractPairedOrder> queue { get; }
    }
}