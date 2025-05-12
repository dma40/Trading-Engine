namespace TradingServer.Orders
{
    public class PairedExecutionOrder: IOrderCore
    {
        public PairedExecutionOrder(IOrderCore orderCore, Order _primary, Order _secondary)
        {
            if (orderCore.OrderType != OrderTypes.PairedExecution)
                throw new InvalidDataException();

            if (_primary.isBuySide != _secondary.isBuySide)
                throw new InvalidDataException();

            _orderCore = orderCore;

            primary = _primary;
            secondary = _secondary;
        }

        public readonly Order primary;
        public readonly Order secondary;

        public long OrderID => _orderCore.OrderID;
        public string Username => _orderCore.Username;
        public string SecurityID => _orderCore.SecurityID;
        public bool isHidden => _orderCore.isHidden;
        public OrderTypes OrderType => _orderCore.OrderType;

        private readonly IOrderCore _orderCore;
    }
}