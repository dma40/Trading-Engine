using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public interface IMatchingOrderbook: IRetrievalOrderbook
    {
        Trades matchIncoming(Order order);
        public bool canFill(Order order);
        public SortedSet<Limit> getAskLimits();
        public SortedSet<Limit> getBidLimits();
    }
}