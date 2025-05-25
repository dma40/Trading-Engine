using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public class PairedOrderRouter
    {
        public PairedOrderRouter()
        {
            routes = new Dictionary<OrderTypes, IPairedRouter>(); // process the market queue first!

            routes.Add(key: OrderTypes.LimitOnOpen, value: new PairedExecutionRouter());
            routes.Add(key: OrderTypes.MarketOnOpen, value: new PairedCancelRouter());
        }

        public void Route(AbstractPairedOrder order)
        {
            if (routes.TryGetValue(order.OrderType, out IPairedRouter? strategy))
            {
                strategy.Route(order); // check if this works okay
            }
        }

        public void Remove(CancelOrder cancel)
        {

        }
        private Dictionary<OrderTypes, IPairedRouter> routes;
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

        }
        public readonly Dictionary<long, AbstractPairedOrder> queue;
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

        }
        public readonly Dictionary<long, AbstractPairedOrder> queue;
    }

    public interface IPairedRouter
    {
        public void Route(AbstractPairedOrder paired);
        public void Remove(CancelOrder cancel);
    }
}