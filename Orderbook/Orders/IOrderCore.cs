using System;

namespace TradingServer.Orders {

    public interface IOrderCore
    {
        public long OrderID { get; }
        public string Username { get; }
        public int SecurityID { get; }
    }
}
