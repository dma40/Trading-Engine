using TradingServer.Instrument;
using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class Orderbook: IRetrievalOrderbook, IDisposable
    {
        private readonly Lock _ordersLock = new();
        private readonly Lock _goodForDayLock = new();
        private readonly Lock _goodTillCancelLock = new();
        private readonly Lock _stopLock = new();

        private bool _disposed = false;
        CancellationTokenSource _ts = new CancellationTokenSource();

        public Orderbook(Security instrument) 
        {
            _instrument = instrument;
            _trades = new Trades();

            _ = Task.Run(() => ProcessAtMarketOpen());
            _ = Task.Run(() => ProcessAtMarketEnd());
            _ = Task.Run(() => ProcessStopOrders());
            _ = Task.Run(() => ProcessTrailingStopOrders());
            _ = Task.Run(() => UpdateGreatestTradedPrice());
        }

        ~Orderbook()
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
    }
}