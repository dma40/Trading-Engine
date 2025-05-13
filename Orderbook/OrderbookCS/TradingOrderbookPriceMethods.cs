namespace TradingServer.OrderbookCS
{
    public partial class TradingEngine: IMatchingEngine, IDisposable
    {
        private static long _greatestTradedPrice = int.MinValue;
        public static long lastTradedPrice { get; private set; }

        protected async Task UpdateGreatestTradedPrice()
        {
            while (true)
            {
                DateTime now = DateTime.Now;
                TimeSpan currentTime = now.TimeOfDay;
                DateTime current = DateTime.Now;

                if (_ts.IsCancellationRequested)
                    return;
                
                if (currentTime <= marketEnd && currentTime >= marketOpen)
                {
                    if (_trades.count >= 0)
                    {
                        var lastTrade = _trades.recordedTrades[_trades.count - 1];
                        lastTradedPrice = lastTrade.tradedPrice;
                    
                        if (lastTrade.tradedPrice > _greatestTradedPrice)
                            _greatestTradedPrice = lastTradedPrice;  
                    } 
                }

                else
                {
                    DateTime tomorrow = current.AddDays(1);
                    DateTime nextTradingDayStart = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 9, 30, 0);
                    TimeSpan closed = nextTradingDayStart - DateTime.Now;

                    await Task.Delay(closed, _ts.Token);
                }

                if (_ts.IsCancellationRequested)
                    return;
            
                await Task.Delay(200, _ts.Token);
            }
        }  
    }
}