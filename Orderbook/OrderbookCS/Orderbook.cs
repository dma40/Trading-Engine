using TradingServer.Instrument;

namespace TradingServer.OrderbookCS
{
    public partial class Orderbook: IOrderEntryOrderbook, IDisposable
    {
        private readonly Security _security;

        CancellationTokenSource _ts = new CancellationTokenSource();
        public bool _disposed => _dispose == 1;
        private int _dispose = 0;

        public Orderbook(Security instrument)
        {
            _security = instrument;
        }

        ~Orderbook()
        {
            Dispose(false);
        }

        public void Dispose() 
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool dispose) 
        {
            if (_disposed) 
                return;
            
            Interlocked.Exchange(ref _dispose, 1);

            if (dispose) 
            {
                _ts.Cancel();
                _ts.Dispose();
            }
        }
    }
}