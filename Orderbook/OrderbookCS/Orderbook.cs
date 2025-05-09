using TradingServer.Instrument;

namespace TradingServer.OrderbookCS
{
    public partial class OrderEntryOrderbook: RetrievalOrderbook, IOrderEntryOrderbook, IDisposable
    {
        private static readonly object _ordersLock = new();
        private readonly object _goodForDayLock = new();
        private readonly object _goodTillCancelLock = new();

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