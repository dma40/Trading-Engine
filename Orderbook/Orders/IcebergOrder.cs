namespace TradingServer.Orders
{
    public class IcebergOrder: Order, IOrderCore
    {
        public IcebergOrder(IOrderCore core, uint _fullQuantity, uint _visibleQuantity, bool _isBuy, long _price):
        base(core, _price, _visibleQuantity, _isBuy)
        {
            if (core.OrderType != OrderTypes.Iceberg)
            {
                throw new InvalidDataException("This is the wrong type");
            }

            if (core.isHidden)
            {
                throw new InvalidDataException("Iceberg orders cannot go to the hidden of the orderbook");
            }

            if (_fullQuantity < _visibleQuantity)
            {
                throw new InvalidDataException("Full quantity cannot be less than visible quantity");
            }

            fullQuantity = _fullQuantity; 
        }

        private uint fullQuantity;

        public bool isEmpty => fullQuantity == 0;

        public void replenish()
        {
            /* This is called only when the current quantity is 0 */
            float total = MathF.Min(Quantity, fullQuantity);

            Random random = new Random();

            var increase = (uint) random.Next(1, (int) total + 1);
            IncreaseQuantity(increase);

            fullQuantity -= Quantity;
            fullQuantity -= increase;
        }
    }
}