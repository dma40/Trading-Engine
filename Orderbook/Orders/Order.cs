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
        public uint Quantity { get; private set; }
        public uint CurrentQuantity { get; private set; }
        public bool isBuySide { get; private set; }

        public void IncreaseQuantity(uint additional) 
        {
            CurrentQuantity += additional;
        }

        public void DecreaseQuantity(uint decrease) 
        {
            if (decrease > CurrentQuantity) 
            {
                throw new InvalidOperationException("You cannot take away more orders than are currently available!");
            }
            CurrentQuantity -= decrease;
        }
    }
}