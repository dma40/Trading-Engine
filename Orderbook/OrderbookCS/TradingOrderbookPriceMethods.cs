namespace TradingServer.OrderbookCS
{
    public partial class TradingOrderbook: OrderEntryOrderbook, ITradingOrderbook, IDisposable
    {
        private long _greatestTradedPrice = int.MinValue;
        public long lastTradedPrice { get; private set; }

        protected async Task UpdateGreatestTradedPrice()
        {
            while (true)
            {
                TimeSpan currentTime = now.TimeOfDay;

                if (_ts.IsCancellationRequested)
                    return;
                
                if (_trades.count > 0 && currentTime <= marketEnd && currentTime >= marketOpen)
                {
                    var lastTrade = _trades.recordedTrades[_trades.count - 1];
                    lastTradedPrice = lastTrade.tradedPrice;
                    
                    if (lastTrade.tradedPrice > _greatestTradedPrice)
                        _greatestTradedPrice = lastTradedPrice;   
                }

                if (_ts.IsCancellationRequested)
                    return;
            
                await Task.Delay(200, _ts.Token);
            }
        }  
    }
}