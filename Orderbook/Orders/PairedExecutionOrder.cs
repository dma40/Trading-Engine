namespace TradingServer.Orders
{
    public class PairedExecutionOrder: AbstractPairedOrder, IOrderCore
    {
        public PairedExecutionOrder(IOrderCore orderCore, Order _primary, Order _secondary): base(orderCore, _primary, _secondary)
        {
            if (orderCore.OrderType != OrderTypes.PairedExecution)
            {
                throw new InvalidDataException();
            }

            if (_primary.isBuySide != _secondary.isBuySide)
            {
                throw new InvalidDataException();
            }

            _orderCore = orderCore;
        }

        private readonly IOrderCore _orderCore;
    }
}