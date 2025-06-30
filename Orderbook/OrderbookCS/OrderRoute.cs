using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public class OrderRoute<T> where T : IOrderCore
    {
        public OrderRoute()
        {
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
}