using TradingServer.Instrument;

namespace TradingServer.OrderbookCS
{
    public partial class MatchingOrderbook: Orderbook, IMatchingOrderbook, IDisposable
    {
        private readonly Lock _ordersLock = new();
        private readonly Lock _stopLock = new();

        private readonly Mutex _orderMutex = new Mutex();
        private readonly Mutex _goodForDayMutex = new Mutex();
        private readonly Mutex _goodTillCancelMutex = new Mutex();

        public MatchingOrderbook(Security security): base(security)
        {
           _trades = new Trades();

           _ = Task.Run(() => ProcessStopOrders());
           _ = Task.Run(() => ProcessTrailingStopOrders());
           _ = Task.Run(() => ProcessAtMarketOpen());
           _ = Task.Run(() => ProcessAtMarketEnd());
           _ = Task.Run(() => UpdateGreatestTradedPrice());
        }

        ~MatchingOrderbook()
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
            { 
                return;
            }

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