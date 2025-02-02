using TradingServer.Instrument;
using TradingServer.Orders;

namespace TradingServer.Orderbook
{
    public class Orderbook: IRetrievalOrderbook
    {
        private readonly Security _instrument;
        private readonly SortedSet<Limit> _askLimits = new SortedSet<Limit>(AskLimitComparer.comparer);
        private readonly SortedSet<Limit> _bidLimits = new SortedSet<Limit>(BidLimitComparer.comparer);
        private readonly Dictionary<long, OrderbookEntry> _orders = new Dictionary<long, OrderbookEntry>();

        public Orderbook(Security instrument) 
        {
            _instrument = instrument;
        }

        public void addOrder(Order order)
        {
            var baseLimit = new Limit(order.Price);
            addOrder(order, baseLimit, order.isBuySide ? _askLimits : _bidLimits, _orders);
        }

        private static void addOrder(Order order, Limit baseLimit, SortedSet<Limit> levels, Dictionary<long, OrderbookEntry> orders)
        {
            OrderbookEntry orderbookEntry = new OrderbookEntry(order, baseLimit);
            if (levels.TryGetValue(baseLimit, out Limit limit))
            {
                
                if (limit.head == null)
                {
                    limit.head = orderbookEntry;
                    limit.tail = orderbookEntry;
                }
                else
                {
                    OrderbookEntry tailPointer = limit.tail;
                    tailPointer.next = orderbookEntry;
                    orderbookEntry.previous = tailPointer;
                    limit.tail = orderbookEntry;
                }
                orders.Add(order.OrderID, orderbookEntry);
            }
            else 
            {
                levels.Add(baseLimit);

                baseLimit.head = orderbookEntry;
                baseLimit.tail = orderbookEntry;

                orders.Add(order.OrderID, orderbookEntry);
            }
        }

        public void removeOrder(CancelOrder cancel)
        {

        }

        public void cancelOrder(CancelOrder cancel)
        {

        }

        public void modifyOrder(ModifyOrder modify)
        {
            if (_orders.TryGetValue(modify.OrderID, out OrderbookEntry orderentry))
            {
                //removeOrder(modify.ToCancelOrder());
            }
        }

        public int count => _orders.Count;
        public List<OrderbookEntry> getAskOrders() => throw new NotImplementedException();
        public List<OrderbookEntry> getBidOrders() => throw new NotImplementedException();
        public bool containsOrder(long orderID)
        {
            return _orders.ContainsKey(orderID);
        }
        public OrderbookSpread spread() => throw new NotImplementedException();
    }
}