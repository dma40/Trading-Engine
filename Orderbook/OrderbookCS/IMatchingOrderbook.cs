using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public interface IMatchingOrderbook: IRetrievalOrderbook
    {
        // matching orderbooks match up orders, and then returing a MatchResult,
        // reporting the results of the match
        Trades match(Order order);
        public bool canFill(Order order);
        public SortedSet<Limit> getAskLimits();
        public SortedSet<Limit> getBidLimits();
    }
}