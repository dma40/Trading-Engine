namespace TradingServer.Orders
{
    public class PairedOrder: IOrderCore
    {
        public PairedOrder(IOrderCore core, Order _primary, Order _secondary)
        {
            if (core.OrderType == OrderTypes.PairedCancel || core.OrderType == OrderTypes.PairedExecution)
            {
                throw new InvalidDataException("Paired orders must have one of the two paired types");
            }

            if (core.isHidden)
            {
                throw new InvalidDataException("Paired orders cannot be hidden");
            }

            _orderCore = core;
            primary = _primary;
            secondary = _secondary;
        }

        public readonly Order primary;
        public readonly Order secondary;

        public long OrderID => _orderCore.OrderID;
        public string SecurityID => _orderCore.SecurityID;
        public string Username => _orderCore.Username;
        public bool isHidden => _orderCore.isHidden;
        public OrderTypes OrderType => _orderCore.OrderType;

        private readonly IOrderCore _orderCore;
    }
}