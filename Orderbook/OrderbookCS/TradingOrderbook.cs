using TradingServer.Instrument;

namespace TradingServer.OrderbookCS
{
    public partial class TradingEngine: IMatchingEngine, IDisposable
    {
        private readonly Lock _ordersLock = new();

        private readonly MatchManager _strategies;
        private readonly OrderRouter _router;
        private readonly PairedOrderRouter _paired;

        public TradingEngine(Security security)
        {
            _trades = new Trades();

            _strategies = new MatchManager(security);
            _router = new OrderRouter();
            _paired = new PairedOrderRouter();

            _ = Task.Run(() => ProcessStopOrders(_ts.Token));
            _ = Task.Run(() => ProcessTrailingStopOrders(_ts.Token));
            _ = Task.Run(() => ProcessAtMarketOpen(_ts.Token));
            _ = Task.Run(() => ProcessAtMarketEnd(_ts.Token));
            _ = Task.Run(() => ProcessPairedCancelOrders(_ts.Token));
            _ = Task.Run(() => ProcessPairedExecutionOrders(_ts.Token));
            _ = Task.Run(() => ProcessIcebergOrders(_ts.Token));
        }

        ~TradingEngine()
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
                _strategies.Dispose();

                _ts.Cancel();
                _ts.Dispose();
            }
        }

        private readonly CancellationTokenSource _ts = new CancellationTokenSource();
        private bool _disposed = false;
    }
}