using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class Orderbook: IOrderEntryOrderbook, IDisposable
    {
        private static readonly Dictionary<long, OrderbookEntry> _orders = new Dictionary<long, OrderbookEntry>();
        private static readonly Dictionary<long, OrderbookEntry> _goodTillCancel = new Dictionary<long, OrderbookEntry>(); 
        private static readonly Dictionary<long, OrderbookEntry> _goodForDay = new Dictionary<long, OrderbookEntry>();
        private static readonly List<int> _supportedOrderTypes = [0, 1, 4];

        public virtual void addOrder(Order order)
        {
            if (!isValidTime(order))
            {
                return;
            }
            
            var baseLimit = new Limit(order.Price);

            if (!_orders.TryGetValue(order.OrderID, out OrderbookEntry? orderbookentry))
            {
                lock (_ordersLock)
                {
                    try
                    {
                        addOrder(order, baseLimit, order.isBuySide ? _bidLimits : _askLimits, _orders);
                    }

                    catch (Exception exception)
                    {
                        Console.WriteLine(exception.Message);
                    }
                }
            }

            else
                throw new InvalidOperationException("This order already exists in the orderbook. You can't add it again");
        }
        
        private void addOrder(Order order, Limit baseLimit, SortedSet<Limit> levels, Dictionary<long, OrderbookEntry> orders)
        {
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
                lock (_goodTillCancelLock)
                    _goodTillCancel.Add(order.OrderID, orderbookEntry);
            }

            if (order.OrderType == OrderTypes.GoodForDay)
            {
                lock (_goodForDayLock)
                    _goodForDay.Add(order.OrderID, orderbookEntry);
            }

            orders.Add(order.OrderID, orderbookEntry);
        }

        protected void removeOrders(List<OrderbookEntry> cancels)
        {
            lock (_ordersLock)
            {
                foreach (OrderbookEntry cancel in cancels)
                    removeOrder(cancel.OrderID, cancel, _orders);
            }
        }

        public virtual void removeOrder(CancelOrder cancel)
        {
            if (!isValidTime(cancel))
            {
                return;
            }

            if (_orders.TryGetValue(cancel.OrderID, out OrderbookEntry? orderbookentry))
            {
                lock (_ordersLock)
                {
                    try
                    {
                        removeOrder(cancel.OrderID, orderbookentry, _orders);
                    }

                    catch (Exception exception)
                    {
                        Console.WriteLine(exception.Message);
                    }
                }
            }

            else
                throw new InvalidOperationException("This order does not exist in the orderbook. You can't remove it");
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
                lock (_goodTillCancelLock)
                    _goodTillCancel.Remove(orderentry.OrderID);
            }
                
            if (orderentry.OrderType == OrderTypes.GoodForDay)
            {
                lock (_goodForDayLock)
                    _goodForDay.Remove(orderentry.OrderID);
            }
            
            orders.Remove(id);
            orderentry.Dispose();
        }

        public virtual void modifyOrder(ModifyOrder modify)
        {
            if (!isValidTime(modify))
            {
                return;
            }

            if (_orders.TryGetValue(modify.OrderID, out OrderbookEntry? orderentry))
            {
                removeOrder(modify.OrderID, orderentry, _orders);
                addOrder(modify.newOrder(), orderentry.ParentLimit, modify.isBuySide ? _bidLimits : _askLimits, _orders);
            }

            else
                throw new InvalidOperationException("This order does not exist in the orderbook. You can't modify it");
        }

        protected virtual bool isValidTime(IOrderCore order)
        {
            int type = (int) order.OrderType;

            if (!_supportedOrderTypes.Contains(type))
                throw new InvalidOperationException("This type of order is not supported by this orderbook");

            if (type == 1)
                return DateTime.Now.Hour < 16 && DateTime.Now.Hour > 9.5;

            return true;
        }
    }
}