using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class Orderbook: IOrderEntryOrderbook
    {
        private readonly Dictionary<long, OrderbookEntry> _orders = new Dictionary<long, OrderbookEntry>();
        private readonly RestingRouter _router;
    
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
                    {
                        orderbookEntry = new OrderbookEntry(order, limit);

                        OrderbookEntry tailPointer = limit.tail;
                        tailPointer.next = orderbookEntry;
                        orderbookEntry.previous = tailPointer;
                        limit.tail = orderbookEntry;
                    }
                }                  
            }

            else 
            {
                levels.Add(baseLimit);

                baseLimit.head = orderbookEntry;
                baseLimit.tail = orderbookEntry;
            }

            _router.Route(orderbookEntry);

            orders.Add(order.OrderID, orderbookEntry);
        }

        private void removeOrders(List<OrderbookEntry> cancels)
        {
            foreach (OrderbookEntry cancel in cancels)
            {
                removeOrder(cancel.OrderID, cancel, _orders);
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
                //lock (_lockManager[orderentry.ParentLimit])
                {
                    orderentry.next.previous = orderentry.previous;
                    orderentry.previous.next = orderentry.next;
                }
            }

            else if (orderentry.ParentLimit.head == orderentry && orderentry.ParentLimit.tail == orderentry)
            {
                //lock (_lockManager[orderentry.ParentLimit])
                {
                    orderentry.ParentLimit.head = null;
                    orderentry.ParentLimit.tail = null;
                }

                if (orderentry.CurrentOrder.isBuySide)
                {
                    _bidLimits.Remove(orderentry.ParentLimit);
                }

                else
                {
                    _askLimits.Remove(orderentry.ParentLimit);
                }
            }

            else if (orderentry.ParentLimit.head == orderentry)
            {
                {
                    if (orderentry.next != null)
                    {
                        orderentry.next.previous = null;
                    }

                    orderentry.ParentLimit.head = orderentry.next;
                }
            }

            else if (orderentry.ParentLimit.tail == orderentry)
            {
                {
                    if (orderentry.previous != null)
                    {
                        orderentry.previous.next = null;
                    }

                    orderentry.ParentLimit.tail = orderentry.previous;
                }
            }

            _router.Remove(orderentry);
            
            orders.Remove(id);
        }

        public void modifyOrder(ModifyOrder modify)
        {
            if (_orders.TryGetValue(modify.OrderID, out OrderbookEntry? orderentry))
            {
                removeOrder(modify.OrderID, orderentry, _orders);
                addOrder(modify.newOrder(), modify.isBuySide ? _bidLimits : _askLimits, _orders);
            }
        }
    }
}