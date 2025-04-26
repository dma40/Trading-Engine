using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class Orderbook: RetrievalOrderbook, IOrderEntryOrderbook, IDisposable
    {
        private readonly Dictionary<long, OrderbookEntry> _orders = new Dictionary<long, OrderbookEntry>();
        private readonly Dictionary<long, OrderbookEntry> _goodTillCancel = new Dictionary<long, OrderbookEntry>(); 
        private readonly Dictionary<long, CancelOrder> _goodForDay = new Dictionary<long, CancelOrder>();

        public virtual void addOrder(Order order)
        {
            lock (_ordersLock) 
            {
                var baseLimit = new Limit(order.Price);

                if (!_orders.TryGetValue(order.OrderID, out OrderbookEntry? orderbookentry)) 
                {
                    addOrder(order, baseLimit, order.isBuySide ? _bidLimits : _askLimits, _orders);
                }
            }
        }
        
        private void addOrder(Order order, Limit baseLimit, SortedSet<Limit> levels, Dictionary<long, OrderbookEntry> orders)
        {
            OrderbookEntry orderbookEntry = new OrderbookEntry(order, baseLimit);

            if (orderbookEntry.CurrentOrder.OrderType == OrderTypes.GoodForDay)
            {
                lock (_goodForDayLock)
                {
                    _goodForDay.Add(order.OrderID, new CancelOrder(order));
                }
            }

            else if (orderbookEntry.CurrentOrder.OrderType == OrderTypes.GoodTillCancel)
            {
                lock (_goodTillCancelLock)
                {
                    _goodTillCancel.Add(order.OrderID, orderbookEntry);
                }
            }

            {
                if (levels.TryGetValue(baseLimit, out Limit? limit) && limit != null)
                {
                    if (limit.head == null)
                    {
                        limit.head = orderbookEntry;
                        limit.tail = orderbookEntry;
                    }

                    else
                    {
                        if (limit.tail != null)
                        {
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

                orders.Add(order.OrderID, orderbookEntry);
            }
        }

        protected void removeOrders(List<CancelOrder> cancels)
        {
            lock (_ordersLock)
            {
                foreach (CancelOrder cancel in cancels)
                {
                    if (_orders.TryGetValue(cancel.OrderID, out OrderbookEntry? orderbookentry) && orderbookentry != null)
                    {
                        removeOrder(cancel.OrderID, orderbookentry, _orders);
                        orderbookentry.Dispose();
                    }
                }
            }
        }

        public virtual void removeOrder(CancelOrder cancel)
        {
            lock (_ordersLock)
            {
                if (_orders.TryGetValue(cancel.OrderID, out OrderbookEntry? orderbookentry) && orderbookentry != null)
                {
                    removeOrder(cancel.OrderID, orderbookentry, _orders);
                    orderbookentry.Dispose();
                }
            }
        }

        private void removeOrder(long id, OrderbookEntry orderentry, Dictionary<long, OrderbookEntry> orders)
        {
            if (orderentry.CurrentOrder.OrderType == OrderTypes.GoodForDay)
            {
                lock (_goodForDayLock)
                {
                    _goodForDay.Remove(id);
                }
            }

            else if (orderentry.CurrentOrder.OrderType == OrderTypes.GoodTillCancel)
            {
                lock (_goodTillCancelLock)
                {
                    _goodTillCancel.Remove(id);
                }
            }

            {
                if (orderentry.previous != null && orderentry.next != null)
                {
                    orderentry.next.previous = orderentry.previous;
                    orderentry.previous.next = orderentry.next;
                }

                else if (orderentry.previous != null)
                {
                    orderentry.previous.next = null;
                }

                else if (orderentry.next != null)
                {
                    orderentry.next.previous = null;
                }

                if (orderentry.ParentLimit.head == orderentry && orderentry.ParentLimit.tail == orderentry)
                {
                    orderentry.ParentLimit.head = null;
                    orderentry.ParentLimit.tail = null;
                
                    if (orderentry.CurrentOrder.isBuySide)
                    {   
                        _bidLimits.Remove(orderentry.ParentLimit);
                    }

                    else 
                    {
                        _askLimits.Remove(orderentry.ParentLimit);
                    }
                }

                else if (orderentry.ParentLimit.head == orderentry && orderentry.ParentLimit.tail != orderentry)
                {
                    orderentry.ParentLimit.head = orderentry.next;
                }

                else if (orderentry.ParentLimit.tail == orderentry)
                {
                    orderentry.ParentLimit.tail = orderentry.previous;
                }

                _orders.Remove(id);
                orderentry.Dispose();
            }
        }

        public virtual void modifyOrder(ModifyOrder modify)
        {   
            lock (_ordersLock)
            {
                if (_orders.TryGetValue(modify.OrderID, out OrderbookEntry? orderentry) && orderentry != null)
                {
                    removeOrder(modify.OrderID, orderentry, _orders);
                    addOrder(modify.newOrder(), orderentry.ParentLimit, modify.isBuySide ? _bidLimits : _askLimits, _orders);
                }
            }
        }
    }
}