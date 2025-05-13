namespace TradingServer.Orders
{
    public class TrailingStopOrder: Order, IOrderCore, IDisposable
    {
        static int ask = int.MinValue;
        static int bid = int.MaxValue;

        public TrailingStopOrder(IOrderCore _orderCore, long _trail, uint _quantity, bool _isBuy): 
        base(_orderCore, _isBuy ? bid : ask, _quantity, _isBuy)
        {
            if (_orderCore.OrderType != OrderTypes.TrailingStopMarket)
                throw new InvalidDataException();

            if (_orderCore.isHidden)
                throw new InvalidDataException();
            
            trail = _trail;
        }

        public TrailingStopOrder(IOrderCore _orderCore, long _trail, long _price, uint _quantity, bool _isBuy):
        base(_orderCore, _price, _quantity, _isBuy)
        {
            if (_orderCore.OrderType != OrderTypes.TrailingStopLimit)
                throw new InvalidDataException();
            
            trail = _trail;
        }

        public long trail { get; private set; }
        
        public long StopPrice 
        {  
            get
            {
                return StopPrice;
            }

            set
            {
                if (isBuySide)
                    StopPrice = currentMaxPrice - trail;

                else
                    StopPrice = currentMaxPrice + trail;
            } 
        }

        public long currentMaxPrice { get; set; }

        public new Order activate()
        {
            return new Order(this);
        }

        ~TrailingStopOrder()
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
                return;
            
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