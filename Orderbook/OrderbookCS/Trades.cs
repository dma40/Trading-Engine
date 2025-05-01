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

        public void addTransactions(Trades other)
        {
            result.AddRange(other.recordedTrades);
        }

        private List<Trade> result;

        public List<Trade> recordedTrades => result;
        public int count => result.Count;
    }
}