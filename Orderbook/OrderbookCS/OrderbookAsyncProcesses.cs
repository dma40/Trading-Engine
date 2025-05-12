using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class Orderbook: IOrderEntryOrderbook, IDisposable
    {
        public void DeleteGoodForDayOrders()
        {
            removeOrders(_goodForDay.Values.ToList());
        }

        public void DeleteExpiredGoodTillCancel()
        {
            List<OrderbookEntry> goodTillCancelOrders = new List<OrderbookEntry>();

            foreach (var order in _goodTillCancel)
            {
                if ((DateTime.UtcNow - order.Value.CreationTime).TotalDays >= 90)
                    goodTillCancelOrders.Add(order.Value);

                removeOrders(goodTillCancelOrders);
            }
        }
    }
}