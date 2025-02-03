namespace TradingServer.Orderbook
{
    public interface IMatchingOrderbook: IRetrievalOrderbook
    {
        // matching orderbooks match up orders, and then returing a MatchResult,
        // reporting the results of the match
        MatchResult match();
    }
}