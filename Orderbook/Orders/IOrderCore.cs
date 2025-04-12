using System;

namespace TradingServer.Orders {

    public interface IOrderCore
    {
        public long OrderID { get; }
        public string Username { get; }
        public string SecurityID { get; }
        public OrderTypes OrderType { get; }
    }
}
