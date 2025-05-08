using TradingServer.Instrument;

namespace TradingServer.OrderbookCS
{
    public partial class TradingOrderbook: OrderEntryOrderbook, ITradingOrderbook, IDisposable
    {
        private readonly Lock _ordersLock = new();
        private readonly Lock _stopLock = new();

        public TradingOrderbook(Security security): base(security)
        {
            _trades = new Trades();

            _ = Task.Run(() => UpdateGreatestTradedPrice());
            _ = Task.Run(() => ProcessStopOrders());
            _ = Task.Run(() => ProcessTrailingStopOrders());
            _ = Task.Run(() => ProcessAtMarketOpen());
            _ = Task.Run(() => ProcessAtMarketEnd());
        }

        ~TradingOrderbook()
        {
            Dispose();
        }

        public new void Dispose() 
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected sealed override void Dispose(bool dispose) 
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

        private readonly CancellationTokenSource _ts = new CancellationTokenSource();
        private bool _disposed = false;
    }
}