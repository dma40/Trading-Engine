using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class TradingEngine: IMatchingEngine, IDisposable
    {
        private long _greatestTradedPrice = -1;
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
                    long price = trades[_trades.count - 1].tradedPrice;

                    if (price > _greatestTradedPrice)
                    {
                        _greatestTradedPrice = price;
                    }

                    return price;
                }
            }
        }
    }
}