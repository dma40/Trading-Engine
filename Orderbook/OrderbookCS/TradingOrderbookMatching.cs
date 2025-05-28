using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public partial class TradingEngine: IMatchingEngine, IDisposable
    {
        private readonly Trades _trades;
        
        public Trades match(Order order)
        {
            Trades result = _strategies.match(order);
            _trades.addTransactions(result);
            return result;
        }
    }
}