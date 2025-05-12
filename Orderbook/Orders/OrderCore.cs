namespace TradingServer.Orders 
{
    public struct OrderCore: IOrderCore
    {
        public OrderCore(long orderID, string username, string securityID, OrderTypes _orderType) 
        {
            OrderID = orderID;
            Username = username;
            SecurityID = securityID;
            OrderType = _orderType;
        }

        public long OrderID { get; private set; }
        public string Username { get; private set; }
        public string SecurityID { get; private set; }
        public bool isHidden => isHidden;
        public OrderTypes OrderType { get; private set; }
    }
}