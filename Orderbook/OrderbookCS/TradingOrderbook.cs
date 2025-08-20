using TradingServer.Instrument;
using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class TradingEngine: IMatchingEngine, IDisposable
    {
        private readonly Lock _ordersLock = new();

        private readonly MatchManager _strategies;
        private readonly OrderRouter _router;

        public TradingEngine(Security security)
        {
            _trades = new Trades();

            _strategies = new MatchManager(security);
            _router = new OrderRouter();

            _ = Task.Run(() => ProcessAtMarketOpen(_ts.Token));
            _ = Task.Run(() => ProcessAtMarketEnd(_ts.Token));
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