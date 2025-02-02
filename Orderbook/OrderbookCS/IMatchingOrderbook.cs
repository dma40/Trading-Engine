namespace TradingServer.Orderbook
{
    public interface IMatchingOrderbook: IRetrievalOrderbook
    {
        MatchResult match();
    }
}