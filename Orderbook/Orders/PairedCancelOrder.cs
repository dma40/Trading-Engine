namespace TradingServer.Orders
{
    public class PairedCancelOrder : AbstractPairedOrder, IOrderCore
    {
        public PairedCancelOrder(IOrderCore orderCore, Order _primary, Order _secondary) : base(orderCore, _primary, _secondary)
        {
            if (orderCore.OrderType != OrderTypes.PairedCancel)
            {
                throw new InvalidDataException("You can't instantiate a PairedCancelOrder like this");
            }

            if (_primary.isBuySide != _secondary.isBuySide)
            {
                throw new InvalidDataException("The two orders need to be of the same side");
            }
        }

        ~PairedCancelOrder()
        {
            
        }
    }
}