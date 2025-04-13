using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public interface IMatchingOrderbook: IRetrievalOrderbook
    {
        // matching orderbooks match up orders, and then returing a MatchResult,
        // reporting the results of the match
        MatchResult match(Order order); // or a IOrderCore object 
        public bool canMatch();
        public SortedSet<Limit> getAskLimits();
        public SortedSet<Limit> getBidLimits();
    }
}