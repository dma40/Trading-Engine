using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class Orderbook: IOrderEntryOrderbook
    {
        internal void DeleteGoodForDayOrders()
        {
            OrderRoute<OrderbookEntry> goodForDay = _router.goodForDay;
            Dictionary<long, OrderbookEntry> gfdQueue = goodForDay.queue;
            List<OrderbookEntry> cancels = gfdQueue.Values.ToList();

            removeOrders(cancels);
        }

        internal void DeleteExpiredGoodTillCancel()
        {
            OrderRoute<OrderbookEntry> goodTillCancel = _router.goodTillCancel;
            Dictionary<long, OrderbookEntry> gtcQueue = goodTillCancel.queue;
            List<OrderbookEntry> gtcOrders = gtcQueue.Values.ToList();

            List<OrderbookEntry> cancels = new List<OrderbookEntry>();

            foreach (var order in gtcOrders)
            {
                if ((DateTime.UtcNow - order.CreationTime).TotalDays >= 90)
                {
                    cancels.Add(order);
                }

                removeOrders(cancels);
            }
        }
    }
}