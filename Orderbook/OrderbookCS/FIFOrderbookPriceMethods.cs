using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class MatchingOrderbook: Orderbook, IMatchingOrderbook, IDisposable
    {
        private long _greatestTradedPrice = Int32.MinValue;
        public long _lastTradedPrice { get; }

         protected async Task UpdateGreatestTradedPrice()
        {
            while (true)
            {
                TimeSpan currentTime = now.TimeOfDay;

                if (_ts.IsCancellationRequested)
                {
                    return;
                }

                if (_trades.result.Count > 0 && currentTime <= marketEnd && currentTime >= marketOpen)
                {
                    var lastTrade = _trades.result[_trades.result.Count - 1];
                    if (lastTrade.tradedPrice > _greatestTradedPrice)
                    {
                        _greatestTradedPrice = lastTrade.tradedPrice;
                    }
                }

                if (_ts.IsCancellationRequested)
                {
                    return;
                }

                await Task.Delay(200, _ts.Token);
            }
        }  
    }
}