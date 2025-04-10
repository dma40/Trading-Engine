using System;

namespace TradingServer.Orders 
{
    public class CancelOrder: IOrderCore 
    {
        public CancelOrder(IOrderCore core)
        {
            _orderCore = core;
        }

        private readonly IOrderCore _orderCore;
        public long OrderID => _orderCore.OrderID;
        public string Username => _orderCore.Username;
        public string SecurityID => _orderCore.SecurityID;
    }
}