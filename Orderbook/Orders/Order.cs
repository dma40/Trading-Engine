using System.Security;

namespace TradingServer.Orders 
{
    // we should also probably store when a order came into the orderbook
    public class Order: IOrderCore 
    {
        public Order(IOrderCore orderCore, long price, uint quantity, bool isBuy, OrderTypes orderType) 
        {
            _orderCore = orderCore;
            Price = price;
            Quantity = quantity;
            CurrentQuantity = quantity;
            isBuySide = isBuy;
            OrderType = orderType;
        }

        public Order(ModifyOrder modify): this(modify, 
        modify.ModifyPrice, modify.ModifyQuantity, modify.isBuySide, modify.OrderType) {}

        private readonly IOrderCore _orderCore;
        
        public long OrderID => _orderCore.OrderID; 
        public string SecurityID => _orderCore.SecurityID;
        public string Username => _orderCore.Username;
        public long Price { get; private set; }
        public OrderTypes OrderType { get; private set; }

        public uint Quantity { get; private set; }
        public uint CurrentQuantity { get; private set; }
        public bool isBuySide { get; private set; }

        public OrderTypes StringToOrderType(string input)
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
                return OrderTypes.IntermediateOrCancel;
            }

            else 
            {
                return OrderTypes.GoodForDay;    
            }

        throw new InvalidOperationException("You cannot have this as a input, this is not in the enum");
    }

        public void IncreaseQuantity(uint additional) 
        {
            // Add some more to your order. 
            CurrentQuantity += additional;
        }

        public void DecreaseQuantity(uint decrease) 
        {
            // Removes the number of securities exchanged by this order. In the LimitOrderbook, we will
            // use this frequently because we will need to match orders, which decreases the amount of 
            // quantity available for a given order.

            // Careful! When dealing with uints, if we decrease
            // by more than the existing quantity, it "loops over" to the 
            // maximum value of a unsigned int and subtracts from that resultant quantity,
            // meaning that we need to prevent negative prices from occurring - this can 
            // lead to serious consequences. 
            
            if (decrease > CurrentQuantity) 
            {
                throw new InvalidOperationException("You cannot take away more orders than are currently available!");
            }
            CurrentQuantity -= decrease;
        }

        public uint remainingQuantity()
        {
            return Quantity - CurrentQuantity;
        }
    }
}