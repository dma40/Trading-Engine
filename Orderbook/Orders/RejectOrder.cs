using TradingServer.Orders;

namespace TradingServer.Rejects 
{
    public class Reject: IOrderCore 
    {
        public Reject(IOrderCore orderCore, RejectionReason rejectionReason) 
        {
            _orderCore = orderCore;
            reason = rejectionReason;
        }

        public RejectionReason reason { get; private set; }
        public long OrderID => _orderCore.OrderID;
        public string Username => _orderCore.Username;
        public int SecurityID => _orderCore.SecurityID;

        private readonly IOrderCore _orderCore;
    }
}