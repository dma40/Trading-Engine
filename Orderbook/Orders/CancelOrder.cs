
namespace TradingServer.Orders 
{
    public class CancelOrder: IOrderCore 
    {
        public CancelOrder(IOrderCore core)
        {
            _orderCore = core;
            CreationTime = DateTime.UtcNow;
        }

        private readonly IOrderCore _orderCore;
        public long OrderID => _orderCore.OrderID;
        public string Username => _orderCore.Username;
        public string SecurityID => _orderCore.SecurityID;
        public DateTime CreationTime { get; private set; }
        public OrderTypes OrderType => _orderCore.OrderType;
    }
}