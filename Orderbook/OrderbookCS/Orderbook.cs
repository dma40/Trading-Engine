using TradingServer.Instrument;

namespace TradingServer.OrderbookCS
{
    public partial class OrderEntryOrderbook: RetrievalOrderbook, IOrderEntryOrderbook, IDisposable
    {
        private readonly Lock _ordersLock = new();
        private readonly Lock _goodForDayLock = new();
        private readonly Lock _goodTillCancelLock = new();

        private bool _disposed = false;
        CancellationTokenSource _ts = new CancellationTokenSource();

        public OrderEntryOrderbook(Security instrument): base(instrument) 
        {
            _ = Task.Run(() => ProcessAtMarketEnd());
        }

        ~OrderEntryOrderbook()
        {
            Dispose();
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
            
            _disposed = true;

            if (dispose) 
            {
                _ts.Cancel();
                _ts.Dispose();
            }
        }
    }
}