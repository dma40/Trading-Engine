using System.Security;

namespace TradingServer.Orders {
    public class Order: IOrderCore 
    {
        public Order(IOrderCore orderCore, long price, uint quantity, bool isBuy) 
        {
            _orderCore = orderCore;
            Price = price;
            Quantity = quantity;
            CurrentQuantity = quantity;
            isBuySide = isBuy;
        }

        public Order(ModifyOrder modify): this(modify, 
        modify.ModifyPrice, modify.ModifyQuantity, modify.isBuySide) {}

        private readonly IOrderCore _orderCore;

        public long OrderID => _orderCore.OrderID; 
        public int SecurityID => _orderCore.SecurityID;
        public string Username => _orderCore.Username;
        public long Price { get; private set;}

        // Important: the initial quantity is the initial amount of orders placed.
        // This may become useful for certain things we want to do, for example, keeping 
        // track of how many excess orders there are (if any)
        
        public uint Quantity { get; private set; }
        public uint CurrentQuantity { get; private set; }
        public bool isBuySide { get; private set; }

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