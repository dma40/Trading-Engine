using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class Orderbook: IOrderEntryOrderbook, IDisposable
    {
        private readonly Dictionary<long, OrderbookEntry> _orders = new Dictionary<long, OrderbookEntry>();
        private readonly Dictionary<long, OrderbookEntry> _goodTillCancel = new Dictionary<long, OrderbookEntry>(); 
        private readonly Dictionary<long, OrderbookEntry> _goodForDay = new Dictionary<long, OrderbookEntry>();
    
        public void addOrder(Order order)
        {
            if (!_orders.TryGetValue(order.OrderID, out OrderbookEntry? orderbookentry))
            {
                addOrder(order, order.isBuySide ? _bidLimits : _askLimits, _orders);
            }
        }
        
        private void addOrder(Order order, SortedSet<Limit> levels, Dictionary<long, OrderbookEntry> orders)
        {
            Limit baseLimit = new Limit(order.Price);
            OrderbookEntry orderbookEntry = new OrderbookEntry(order, baseLimit);

            if (levels.TryGetValue(baseLimit, out Limit? limit))
            {
                if (limit.tail != null)
                {
                    orderbookEntry = new OrderbookEntry(order, limit);

                    OrderbookEntry tailPointer = limit.tail;
                    tailPointer.next = orderbookEntry;
                    orderbookEntry.previous = tailPointer;
                    limit.tail = orderbookEntry;
                }                  
            }

            else 
            {
                levels.Add(baseLimit);

                baseLimit.head = orderbookEntry;
                baseLimit.tail = orderbookEntry;
            }

            if (order.OrderType == OrderTypes.GoodTillCancel)
            {
                _goodTillCancel.Add(order.OrderID, orderbookEntry);
            }

            if (order.OrderType == OrderTypes.GoodForDay)
            {
                _goodForDay.Add(order.OrderID, orderbookEntry);
            }

            orders.Add(order.OrderID, orderbookEntry);
        }

        private void removeOrders(List<OrderbookEntry> cancels)
        {
            foreach (OrderbookEntry cancel in cancels)
                removeOrder(cancel.OrderID, cancel, _orders);
        }

        private void removeOrders(List<CancelOrder> cancels)
        {
            foreach (CancelOrder cancel in cancels)
            {
                removeOrder(cancel);
            }
        }

        public void removeOrder(CancelOrder cancel)
        {
            if (_orders.TryGetValue(cancel.OrderID, out OrderbookEntry? orderbookentry))
            {
                removeOrder(cancel.OrderID, orderbookentry, _orders);
            }
        }

        private void removeOrder(long id, OrderbookEntry orderentry, Dictionary<long, OrderbookEntry> orders)
        {
            if (orderentry.previous != null && orderentry.next != null)
            {
                orderentry.next.previous = orderentry.previous;
                orderentry.previous.next = orderentry.next;
            }
            
            else if (orderentry.ParentLimit.head == orderentry && orderentry.ParentLimit.tail == orderentry)
            {
                orderentry.ParentLimit.head = null;
                orderentry.ParentLimit.tail = null;
                
                if (orderentry.CurrentOrder.isBuySide)
                    _bidLimits.Remove(orderentry.ParentLimit);

                else 
                    _askLimits.Remove(orderentry.ParentLimit);    
            }

            else if (orderentry.ParentLimit.head == orderentry)
            {
                if (orderentry.next != null)
                    orderentry.next.previous = null;

                orderentry.ParentLimit.head = orderentry.next;
            }
            
            else if (orderentry.ParentLimit.tail == orderentry)
            {
                if (orderentry.previous != null)
                    orderentry.previous.next = null;

                orderentry.ParentLimit.tail = orderentry.previous;
            }

            if (orderentry.OrderType == OrderTypes.GoodTillCancel)
            {
                _goodTillCancel.Remove(orderentry.OrderID);
            }
                
            if (orderentry.OrderType == OrderTypes.GoodForDay)
            {
                _goodForDay.Remove(orderentry.OrderID);
            }
            
            orders.Remove(id);
            orderentry.Dispose();
        }

        public void modifyOrder(ModifyOrder modify)
        {
            if (_orders.TryGetValue(modify.OrderID, out OrderbookEntry? orderentry))
            {
                removeOrder(modify.OrderID, orderentry, _orders);
                addOrder(modify.newOrder(), modify.isBuySide ? _bidLimits : _askLimits, _orders);
            }
        }

        public static bool isValidTime(IOrderCore order)
        {
            if (order.OrderType == OrderTypes.GoodForDay)
                return DateTime.Now.Hour < 16 && DateTime.Now.Hour > 9.5;
            
            else
                return true;
        }
    }
}