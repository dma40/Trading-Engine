using System.Security.Authentication.ExtendedProtection;

namespace TradingServer.Orders 
{
    public struct CancelOrder : IOrderCore
    {
        public CancelOrder(IOrderCore core)
        {
            _orderCore = core;
        }

        public CancelOrder(IOrderCore core, long childID)
        {
            _orderCore = core;
            ChildID = childID;
        }

        private readonly IOrderCore _orderCore;
        public long OrderID => _orderCore.OrderID;
        public string Username => _orderCore.Username;
        public string SecurityID => _orderCore.SecurityID;
        public bool isHidden => _orderCore.isHidden;
        public OrderTypes OrderType => _orderCore.OrderType;
        public long ChildID;
    }
}