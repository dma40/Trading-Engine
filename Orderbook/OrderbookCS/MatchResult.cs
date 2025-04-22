using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public sealed class Trades
    {
        public Trades() 
        {
            result = new List<Trade>();
        }

        public void addTransaction(Trade trade)
        {
            result.Add(trade);
        }

        public List<Trade> result { get; }
    }
}