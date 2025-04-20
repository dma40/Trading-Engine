using TradingServer.Orders;

namespace TradingServer.OrderbookCS
{
    public interface IMatchingOrderbook: IRetrievalOrderbook
    {
        Trades match(Order order);
        public bool canFill(Order order);
        public SortedSet<Limit> getAskLimits();
        public SortedSet<Limit> getBidLimits();
    }
}