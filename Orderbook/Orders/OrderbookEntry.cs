namespace TradingServer.Orders 
{
    public class Limit 
    {
        public Limit(long price)
        {
            Price = price;
        }
        public long Price { get; private set; }
        public OrderbookEntry head { get; set; }
        public OrderbookEntry tail { get; set; }

        public uint getLevelOrderCount() 
        {
            uint count = 0;
            OrderbookEntry headPointer = head;
            while (headPointer != null)
            {
                if (headPointer.CurrentOrder.CurrentQuantity != 0) 
                {
                    count++;
                }
                headPointer = headPointer.next; 
            }
            return count;
        }

        public uint getLevelOrderQuantity() 
        {
            uint count = 0;
            OrderbookEntry headPointer = head;
            while (headPointer != null)
            {
                if (headPointer.CurrentOrder.CurrentQuantity != 0)
                {
                    count += headPointer.CurrentOrder.CurrentQuantity;
                }
                headPointer = headPointer.next;
            }
            return count;
        }

        public List<OrderRecord> getOrderRecords() 
        {
            List<OrderRecord> records = new List<OrderRecord>();
            OrderbookEntry headPointer = head;
            uint queuePosition = 0;
            while (headPointer != null)
            {
                var current = headPointer.CurrentOrder;
                records.Add(new OrderRecord(current.OrderID, current.CurrentQuantity, 
                current.Price, current.isBuySide, 
                current.Username, current.SecurityID, queuePosition));

                queuePosition++;
                headPointer = headPointer.next;
            }
            return records;
        }
        public bool isEmpty 
        {
            get
            {
                return head == null && tail == null;
            }
        }
        public Side side 
        {
            get 
            {
                if (isEmpty) 
                {
                    return Side.Unknown;
                }
                else {
                    return head.CurrentOrder.isBuySide ? Side.Bid : Side.Ask;
                }
            }
        }

    }

    public class OrderbookEntry
    {
        public OrderbookEntry(Order currentOrder, Limit parentLimit)
        {
            CreationTime = DateTime.UtcNow;
            CurrentOrder = currentOrder;
            ParentLimit = parentLimit;
        }

        public DateTime CreationTime { get; private set; }
        
        public Order CurrentOrder { get; private set; }

        public Limit ParentLimit { get; private set; }

        public OrderbookEntry next { get; set; }

        public OrderbookEntry previous { get; set; }
    }
}