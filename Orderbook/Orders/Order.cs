namespace TradingServer.Orders 
{
    // maybe make a new order type
    public class Order: IOrderCore, IDisposable
    {
        public Order(IOrderCore orderCore, long price, uint quantity, bool isBuy, OrderTypes orderType) 
        {
            if (orderType == OrderTypes.Market)
            {
                throw new InvalidDataException("Market orders cannot have a price");
            }

            _orderCore = orderCore;
            Price = price;
            Quantity = quantity;
            CurrentQuantity = quantity;
            isBuySide = isBuy;
            OrderType = orderType;
        }

        public Order(IOrderCore orderCore, uint quantity, bool isBuy, OrderTypes orderType)
        {
            if (orderType != OrderTypes.Market)
            {
                throw new InvalidDataException("Only market orders can have no price");
            }

            _orderCore = orderCore;
            Quantity = quantity;
            isBuySide = isBuy;
            OrderType = OrderTypes.Market;

            if (isBuySide)
            {
                Price = Int32.MaxValue;
            }

            else 
            {
                Price = Int32.MinValue;
            }
        }

        public Order(ModifyOrder modify): this(modify, 
            modify.ModifyPrice, modify.ModifyQuantity, modify.isBuySide, modify.OrderType) {}

        public Order(StopOrder stop): this(stop, stop.limitPrice, stop.Quantity, 
            stop.isBuySide, stop.OrderType) {}

        public Order(TrailingStopOrder trail): this(trail, trail.StopPrice, trail.Quantity, 
            trail.isBuySide, trail.OrderType) {}

        private readonly IOrderCore _orderCore;
        
        public long OrderID => _orderCore.OrderID; 
        public string SecurityID => _orderCore.SecurityID;
        public string Username => _orderCore.Username;
        public long Price { get; private set; }
        public OrderTypes OrderType { get; private set; }

        public uint Quantity { get; private set; }
        public uint CurrentQuantity { get; private set; }
        public bool isBuySide { get; private set; }

        public static OrderTypes StringToOrderType(string input)
        {
            if (input == "FillOrKill")
            {
                return OrderTypes.FillOrKill;
            }

            else if (input == "GoodTillCancel")
            {
                return OrderTypes.GoodTillCancel;
            }

            else if (input == "IntermediateOrCancel")
            {
                return OrderTypes.FillAndKill;
            }

            else if (input == "PostOnly")
            {
                return OrderTypes.PostOnly;
            }

            else 
            {
                return OrderTypes.GoodForDay;    
            }

            throw new InvalidOperationException("You cannot have this as a input, this is not in the enum");
        }

        public void IncreaseQuantity(uint additional) 
        {
            CurrentQuantity += additional;
        }

        public void DecreaseQuantity(uint decrease) 
        {
            if (decrease > CurrentQuantity) 
            {
                throw new InvalidOperationException("You cannot take away more orders than are currently open!");
            }
            CurrentQuantity -= decrease;
        }

        public uint remainingQuantity()
        {
            return Quantity - CurrentQuantity;
        }

        ~Order()
        {
            Dispose();
        }

        public void Dispose() 
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool dispose) 
        {
            if (_disposed) 
            { 
                return;
            }

            _disposed = true;

            if (dispose) 
            {
                _ts.Cancel();
                _ts.Dispose();
            }
        }

        private bool _disposed = false;
        CancellationTokenSource _ts = new CancellationTokenSource();
    }
}