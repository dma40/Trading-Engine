using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class OrderEntryOrderbook: RetrievalOrderbook, IOrderEntryOrderbook, IDisposable
    {
        private readonly Dictionary<long, OrderbookEntry> _orders = new Dictionary<long, OrderbookEntry>();
        private readonly Dictionary<long, OrderbookEntry> _goodTillCancel = new Dictionary<long, OrderbookEntry>(); 
        private readonly Dictionary<long, CancelOrder> _goodForDay = new Dictionary<long, CancelOrder>();

        public virtual void addOrder(Order order)
        {
            var baseLimit = new Limit(order.Price);
            OrderbookEntry orderbookEntry = new OrderbookEntry(order, baseLimit);

            if (!_orders.TryGetValue(order.OrderID, out OrderbookEntry? orderbookentry)) 
                lock (_ordersLock)
                    addOrder(order, baseLimit, order.isBuySide ? _bidLimits : _askLimits, _orders);

            if (!_goodTillCancel.TryGetValue(order.OrderID, out OrderbookEntry? orderentry) && order.OrderType == OrderTypes.GoodTillCancel)
                lock (_goodTillCancelLock)
                    _goodTillCancel.Add(order.OrderID, orderbookEntry);

            if (!_goodForDay.TryGetValue(order.OrderID, out CancelOrder cancel) && order.OrderType == OrderTypes.GoodForDay)
                lock (_goodForDayLock)
                    _goodForDay.Add(order.OrderID, new CancelOrder(order));
                
            else
                throw new InvalidOperationException();
        }
        
        private void addOrder(Order order, Limit baseLimit, SortedSet<Limit> levels, Dictionary<long, OrderbookEntry> orders)
        {
            OrderbookEntry orderbookEntry = new OrderbookEntry(order, baseLimit);

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
        }

        protected void removeOrders(List<CancelOrder> cancels)
        {
            lock (_ordersLock)
            {
                foreach (CancelOrder cancel in cancels)
                    removeOrder(cancel);
            }
        }

        public virtual void removeOrder(CancelOrder cancel)
        {
            if (_orders.TryGetValue(cancel.OrderID, out OrderbookEntry? orderbookentry) && orderbookentry != null)
                lock (_ordersLock)
                        removeOrder(cancel.OrderID, orderbookentry, _orders);
            else
                throw new InvalidOperationException();
           
            if (_goodTillCancel.TryGetValue(cancel.OrderID, out OrderbookEntry? orderentry) && orderentry != null)
                lock (_goodTillCancelLock)
                    _goodTillCancel.Remove(cancel.OrderID);

                
            if (_goodForDay.TryGetValue(cancel.OrderID, out CancelOrder day))
                lock (_goodForDayLock)
                    _goodForDay.Remove(day.OrderID);

            orderbookentry.Dispose();     
        }

        private void removeOrder(long id, OrderbookEntry orderentry, Dictionary<long, OrderbookEntry> orders)
        {
            if (orderentry.previous != null && orderentry.next != null)
            {
                orderentry.next.previous = orderentry.previous;
                orderentry.previous.next = orderentry.next;
            }

            else if (orderentry.previous != null)
                orderentry.previous.next = null;
            

            else if (orderentry.next != null)
                orderentry.next.previous = null;
            

            else if (orderentry.ParentLimit.head == orderentry && orderentry.ParentLimit.tail == orderentry)
            {
                orderentry.ParentLimit.head = null;
                orderentry.ParentLimit.tail = null;
                
                if (orderentry.CurrentOrder.isBuySide)
                    _bidLimits.Remove(orderentry.ParentLimit);

                else 
                    _askLimits.Remove(orderentry.ParentLimit);    
            }

            else if (orderentry.ParentLimit.head == orderentry && orderentry.ParentLimit.tail != orderentry)
                orderentry.ParentLimit.head = orderentry.next;
            

            else if (orderentry.ParentLimit.tail == orderentry)
                orderentry.ParentLimit.tail = orderentry.previous;
            
            orders.Remove(id);
            orderentry.Dispose();
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

                else
                    throw new InvalidOperationException();
            }
        }
    }
}