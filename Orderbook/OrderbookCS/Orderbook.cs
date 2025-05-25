using TradingServer.Instrument;

namespace TradingServer.OrderbookCS
{
    public partial class Orderbook: IOrderEntryOrderbook, IDisposable
    {
        private readonly Security _security;

        CancellationTokenSource _ts = new CancellationTokenSource();
        private bool _disposed = false;

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
            {
                return;
            }
            
            Interlocked.Exchange(ref _disposed, true);

            if (dispose) 
            {
                _ts.Cancel();
                _ts.Dispose();
            }
        }
    }
}