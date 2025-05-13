namespace TradingServer.Orders
{
    public class IcebergOrder: Order, IOrderCore
    {
        public IcebergOrder(IOrderCore core, uint _fullQuantity, uint _visibleQuantity, bool _isBuy, long _price):
        base(core, _price, _visibleQuantity, _isBuy)
        {
            if (core.OrderType != OrderTypes.Iceberg)
                throw new InvalidDataException("This is the wrong type");

            if (core.isHidden)
                throw new InvalidDataException("Iceberg orders cannot go to the hidden of the orderbook");

            fullQuantity = _fullQuantity; 
        }

        private uint fullQuantity;

        public void replenish()
        {
            /* This is called only when the quantity is 0 */
            float total = MathF.Min(Quantity, fullQuantity);

            Random random = new Random();
            IncreaseQuantity((uint) random.Next(1, (int) total));
        }
    }
}