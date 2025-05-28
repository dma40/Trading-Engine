using System.Runtime.InteropServices;

namespace TradingServer.Orders
{
    public abstract class AbstractPairedOrder : IOrderCore
    {
        protected AbstractPairedOrder(IOrderCore core, Order _primary, Order _secondary)
        {
            _core = core;
            primary = _primary;
            secondary = _secondary;
        }
        
        /*
        public AbstractPairedOrder(PairedCancelOrder other, ModifyOrder modify)
        {
            _core = other;
            primary = modify.OrderID == other.primary.OrderID ? modify.newOrder() : other.primary;
            secondary = modify.OrderID == other.secondary.OrderID ? modify.newOrder() : other.secondary;
        }
        */

        public readonly Order primary;
        public readonly Order secondary;

        private readonly IOrderCore _core;
        public long OrderID => _core.OrderID;
        public string Username => _core.Username;
        public string SecurityID => _core.SecurityID;
        public OrderTypes OrderType => _core.OrderType;
        public bool isHidden => _core.isHidden;
    }
}