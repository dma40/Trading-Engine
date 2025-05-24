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

            Interlocked.Exchange(ref _dispose, 1);
           
            if (dispose)
            {
                orderbook.Dispose();
                _hidden.Dispose();

                _ts.Cancel();
                _ts.Dispose();
            }
        }

        private readonly CancellationTokenSource _ts = new CancellationTokenSource();
        private bool _disposed => _dispose == 1;
        private int _dispose = 0;
    }
}