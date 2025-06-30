using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public class RestingRouter
    {
        public RestingRouter()
        {
            _routes = new Dictionary<OrderTypes, OrderRoute<OrderbookEntry>>();

            _routes.Add(key: OrderTypes.GoodForDay, value: new OrderRoute<OrderbookEntry>());
            _routes.Add(key: OrderTypes.GoodTillCancel, value: new OrderRoute<OrderbookEntry>());
        }

        public void Route(OrderbookEntry orderbookEntry)
        {
            if (_routes.TryGetValue(orderbookEntry.OrderType, out OrderRoute<OrderbookEntry>? resting))
            {
                resting.Route(orderbookEntry);
            }
        }

        public void Remove(OrderbookEntry orderbookEntry)
        {
            if (_routes.TryGetValue(orderbookEntry.OrderType, out OrderRoute<OrderbookEntry>? resting))
            {
                resting.Remove(orderbookEntry);
            }
        }

        public OrderRoute<OrderbookEntry> goodForDay => _routes[OrderTypes.GoodForDay];
        public OrderRoute<OrderbookEntry> goodTillCancel => _routes[OrderTypes.GoodTillCancel];

        private readonly Dictionary<OrderTypes, OrderRoute<OrderbookEntry>> _routes;
    }
}