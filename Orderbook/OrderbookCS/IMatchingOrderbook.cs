using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    // Interface useful when making matching orderbooks in the future
    public interface IMatchingOrderbook: IRetrievalOrderbook
    {
        // matching orderbooks match up orders, and then returing a MatchResult,
        // reporting the results of the match
        MatchResult match();
        public bool canMatch();
        public SortedSet<Limit> getAskLimits();
        public SortedSet<Limit> getBidLimits();
        // a match method will match bid/ask orders from both sides
        // of the orderbook. We can use different criteria for sorting orders
        // because we have defined many ways to select orders from the bid/ask sides.
    }
}