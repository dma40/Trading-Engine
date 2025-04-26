namespace TradingServer.OrderbookCS
{
    public partial class MatchingOrderbook: Orderbook, IMatchingOrderbook, IDisposable
    {
        private long _greatestTradedPrice = int.MinValue;
        public long _lastTradedPrice { get; private set; }

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
                    _lastTradedPrice = lastTrade.tradedPrice;
                    
                    if (lastTrade.tradedPrice > _greatestTradedPrice)
                    {
                        _greatestTradedPrice = _lastTradedPrice;
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