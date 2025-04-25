using TradingServer.Instrument;
using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class MatchingOrderbook: Orderbook, IMatchingOrderbook, IDisposable
    {
        private readonly Dictionary<long, StopOrder> _stop = new Dictionary<long, StopOrder>();
        private readonly Dictionary<long, TrailingStopOrder> _trailingStop = new Dictionary<long, TrailingStopOrder>();

        private DateTime now;
        private static readonly TimeSpan marketOpen = new TimeSpan(9, 30, 0);
        private static readonly TimeSpan marketEnd = new TimeSpan(16, 0, 0);
        
        private Trades _trades;
        private long _greatestTradedPrice = Int32.MinValue;
        private long _lastTradedPrice;

        private readonly Lock _ordersLock = new();
        private readonly Lock _goodForDayLock = new();
        private readonly Lock _goodTillCancelLock = new();
        private readonly Lock _stopLock = new();

        private readonly Mutex _orderMutex = new Mutex();
        private readonly Mutex _goodForDayMutex = new Mutex();
        private readonly Mutex _goodTillCancelMutex = new Mutex();

        private readonly Dictionary<long, Order> _onMarketOpen = new Dictionary<long, Order>();
        private readonly Dictionary<long, Order> _onMarketClose = new Dictionary<long, Order>();

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