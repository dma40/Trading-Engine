using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class TradingEngine: IMatchingEngine, IDisposable
    {
        private long _greatestTradedPrice = int.MinValue;
        public long lastTradedPrice
        {
            get
            {
                if (_trades.count > 0)
                {
                    return -1;
                }

                else
                {
                    List<Trade> trades = _trades.recordedTrades;
                    return trades[_trades.count - 1].tradedPrice;
                }
            }
        }
    }
}