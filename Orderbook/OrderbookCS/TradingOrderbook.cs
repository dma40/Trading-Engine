using TradingServer.Instrument;

namespace TradingServer.OrderbookCS
{
    public partial class TradingEngine: IMatchingEngine, IDisposable
    {
        private readonly Lock _ordersLock = new();
        private readonly Lock _stopLock = new();
        public readonly Orderbook orderbook;
        private readonly Orderbook _hidden;

        public TradingEngine(Security security)
        {
            _trades = new Trades();

            orderbook = new Orderbook(security);
            _hidden = new Orderbook(security);

            _ = Task.Run(() => UpdateGreatestTradedPrice());

            _ = Task.Run(() => ProcessStopOrders());
            _ = Task.Run(() => ProcessTrailingStopOrders());
            _ = Task.Run(() => ProcessAtMarketOpen());
            _ = Task.Run(() => ProcessAtMarketEnd());

            _ = Task.Run(() => ProcessPairedCancelOrders());
            _ = Task.Run(() => ProcessPairedExecutionOrders());
        }

        ~TradingEngine()
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

        private readonly CancellationTokenSource _ts = new CancellationTokenSource();
        private bool _disposed = false;
    }
}