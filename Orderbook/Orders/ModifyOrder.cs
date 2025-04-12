using System;

namespace TradingServer.Orders 
{
    public class ModifyOrder: IOrderCore 
    {
        public ModifyOrder(IOrderCore orderCore, long price, uint quantity, bool isBuy) 
        {
            _orderCore = orderCore;
            ModifyPrice = price;
            ModifyQuantity = quantity;
            isBuySide = isBuy;

        }
        public string SecurityID => _orderCore.SecurityID;
        public string Username => _orderCore.Username;
        public long OrderID => _orderCore.OrderID;
        public OrderTypes OrderType => _orderCore.OrderType;
        public long ModifyPrice { get; private set; }
        public uint ModifyQuantity { get; private set; }

        public bool isBuySide { get; private set; }
        private readonly IOrderCore _orderCore;

        public CancelOrder cancelOrder() 
        {
            return new CancelOrder(_orderCore);
        }

        public Order newOrder() 
        {
            // return new Order(_orderCore, ModifyPrice, ModifyQuantity, isBuySide);
            return new Order(this);
        }
    }
}