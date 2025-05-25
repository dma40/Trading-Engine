namespace TradingServer.Orders
{
    public class StopOrder: Order, IOrderCore, IDisposable
    {
        public StopOrder(IOrderCore _orderCore, long _price, uint _quantity, bool _isBuy):
        base(_orderCore, _price, _quantity, _isBuy)
        {
            if (OrderType != OrderTypes.StopMarket)
            {
                throw new InvalidDataException();
            }

            if (_orderCore.isHidden)
            {
                throw new InvalidDataException("Stop market orders can never go to the hidden portion of the orderbook");
            }

            if (isBuySide)
            {
                limitPrice = int.MaxValue;
            }

            else
            {
                limitPrice = -1;
            }
        }

        public StopOrder(IOrderCore _orderCore, long _price, long _limitPrice, uint _quantity, bool _isBuy): 
        base(_orderCore, _price, _quantity, _isBuy)
        {
            if (OrderType != OrderTypes.StopLimit)
            {
                throw new InvalidDataException();
            }
            
            limitPrice = _limitPrice;
        }

        public long limitPrice { get; private set; }

        public long StopPrice => Price;

        public sealed override Order activate()
        {
            return new Order(this);
        }

         ~StopOrder()
        {
            Dispose(false);
        }

        public new void Dispose() 
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
        private readonly CancellationTokenSource _ts = new CancellationTokenSource();
    }
}