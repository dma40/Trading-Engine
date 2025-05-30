using TradingServer.Orders;

namespace TradingServer.Rejects 
{
    public sealed class Reject: IOrderCore 
    {
        public Reject(IOrderCore orderCore, RejectionReason rejectionReason) 
        {
            _orderCore = orderCore;
            reason = rejectionReason;
        }

        public RejectionReason reason { get; private set; }
        public long OrderID => _orderCore.OrderID;
        public string Username => _orderCore.Username;
        public string SecurityID => _orderCore.SecurityID;
        public bool isHidden => _orderCore.isHidden;
        public OrderTypes OrderType => _orderCore.OrderType;

        private readonly IOrderCore _orderCore;
    }
}