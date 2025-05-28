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
            isHidden = false;
        }

        public OrderCore(long orderID, string username, string securityID, OrderTypes _orderType, bool _isHidden)
        {
            if (_orderType == OrderTypes.PostOnly && _isHidden)
            {
                throw new InvalidDataException("Post only orders cannot be hidden");
            }

            OrderID = orderID;
            Username = username;
            SecurityID = securityID;
            OrderType = _orderType;
            isHidden = _isHidden;
        }

        public long OrderID { get; private set; }
        public string Username { get; private set; }
        public string SecurityID { get; private set; }
        public OrderTypes OrderType { get; private set; }
        public bool isHidden { get; private set; }
        
    }
}