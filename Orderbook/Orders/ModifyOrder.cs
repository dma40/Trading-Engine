namespace TradingServer.Orders 
{
    public struct ModifyOrder: IOrderCore 
    {
        public ModifyOrder(IOrderCore orderCore, long price, uint quantity, bool isBuy)
        {
            _orderCore = orderCore;
            ModifyPrice = price;
            ModifyQuantity = quantity;
            isBuySide = isBuy;
            ChildID = -1; // this is not a paired order, so there is no child id. 
        }

        public ModifyOrder(IOrderCore orderCore, long childID, long price, uint quantity, bool isBuy)
        {
            _orderCore = orderCore;
            ChildID = childID;
            ModifyPrice = price;
            ModifyQuantity = quantity;
            isBuySide = isBuy;
        }
        
        public string SecurityID => _orderCore.SecurityID;
        public string Username => _orderCore.Username;
        public long OrderID => _orderCore.OrderID;
        public bool isHidden => _orderCore.isHidden;
        public OrderTypes OrderType => _orderCore.OrderType;
        public long ModifyPrice { get; private set; }
        public uint ModifyQuantity { get; private set; }
        public long ChildID { get; private set; }

        public bool isBuySide { get; private set; }
        private readonly IOrderCore _orderCore;

        public CancelOrder cancelOrder() 
        {
            return new CancelOrder(_orderCore);
        }

        public Order newOrder() 
        {
            return new Order(this);
        }
    }
}