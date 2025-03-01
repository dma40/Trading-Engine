using TradingServer.Orders;
using TradingServer.Instrument;

namespace TradingServer.OrderbookCS
{
    public class PROrderbook: Orderbook, IMatchingOrderbook
    {
        public PROrderbook(Security security): base(security)
        {
            _security = security;
        }

        public MatchResult match()
        {
            // matching algorithm works like this:
            // take the floor of remaining orders; if there's no more to be matched 
            // then put all the orders into the order still has unmatched orders
            MatchResult match = new MatchResult();
            return match;

        }
        private readonly Security _security;
    }
}