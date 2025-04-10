using System;

namespace TradingServer.Orders {
    public class OrderCore: IOrderCore
    {
        public OrderCore(long orderID, string username, string securityID) 
        {
            OrderID = orderID;
            Username = username;
            SecurityID = securityID;
        }

        public long OrderID { get; private set; }
        public string Username { get; private set; }
        public string SecurityID { get; private set; }

    }
}