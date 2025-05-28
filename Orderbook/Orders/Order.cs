namespace TradingServer.Orders 
{
    public class Order : IOrderCore
    {
        public Order(IOrderCore orderCore, long price, uint quantity, bool isBuy)
        {
            if (orderCore.OrderType == OrderTypes.Market)
            {
                throw new InvalidDataException("Market orders cannot have a price");
            }

            _orderCore = orderCore;
            Price = price;
            Quantity = quantity;
            CurrentQuantity = quantity;
            isBuySide = isBuy;
        }

        public Order(IOrderCore orderCore, uint quantity, bool isBuy)
        {
            if (orderCore.OrderType != OrderTypes.Market)
            {
                throw new InvalidDataException("Only market orders can have no price");
            }

            if (orderCore.isHidden)
            {
                throw new InvalidDataException("Market orders cannot go to the hidden portion");
            }

            _orderCore = orderCore;
            Quantity = quantity;
            isBuySide = isBuy;

            if (isBuySide)
            {
                Price = int.MaxValue;
            }

            else
            {
                Price = -1;
            }
        }

        public Order(ModifyOrder modify) : this(modify,
            modify.ModifyPrice, modify.ModifyQuantity, modify.isBuySide)
        { }

        public Order(StopOrder stop) : this(stop, stop.limitPrice, stop.Quantity,
            stop.isBuySide)
        { }

        public Order(TrailingStopOrder trail) : this(trail, trail.StopPrice, trail.Quantity,
            trail.isBuySide)
        { }

        public CancelOrder cancelOrder()
        {
            return new CancelOrder(_orderCore);
        }

        private readonly IOrderCore _orderCore;

        public long OrderID => _orderCore.OrderID;
        public string SecurityID => _orderCore.SecurityID;
        public string Username => _orderCore.Username;
        public bool isHidden => _orderCore.isHidden;
        public OrderTypes OrderType => _orderCore.OrderType;

        public long Price { get; private set; }
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

        public virtual Order activate()
        {
            return this;
        }

        public virtual void replenish()
        {
            throw new NotImplementedException();
        }

        public virtual bool isEmpty => throw new NotImplementedException();
        public virtual long StopPrice => throw new NotImplementedException();
        public long currentMaxPrice;

        ~Order()
        {
            
        }
    }
}