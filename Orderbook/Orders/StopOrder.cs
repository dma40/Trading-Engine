namespace TradingServer.Orders
{
    public class StopOrder: Order, IOrderCore, IDisposable
    {
        public StopOrder(IOrderCore _orderCore, long _price, uint _quantity, bool _isBuy, OrderTypes _orderType) :
        base(_orderCore, _price, _quantity, _isBuy, _orderType)
        {
            if (OrderType != OrderTypes.StopMarket)
            {
                throw new InvalidDataException();
            }

            if (isBuySide)
            { 
                limitPrice = Int32.MaxValue; 
            }

            else
            {
                limitPrice = Int32.MinValue;
            } 
        }

        public StopOrder(IOrderCore _orderCore, long _price, long _limitPrice, uint _quantity, bool _isBuy, OrderTypes _orderType): 
        base(_orderCore, _price, _quantity, _isBuy, _orderType)
        {
            if (OrderType != OrderTypes.StopLimit)
            {
                throw new InvalidDataException();
            }

            limitPrice = limitPrice;
        }

        public long limitPrice { get; private set; }

        public long stopPrice => Price;

        public Order activate()
        {
            return new Order(this);
        }

         ~StopOrder()
        {
            Dispose();
        }

        public void Dispose() 
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool dispose) 
        {
            if (_disposed) 
            { 
                return;
            }

            _disposed = true;

            if (dispose) 
            {
                _ts.Cancel();
                _ts.Dispose();
            }
        }

        private bool _disposed = false;
        CancellationTokenSource _ts = new CancellationTokenSource();
    }
}