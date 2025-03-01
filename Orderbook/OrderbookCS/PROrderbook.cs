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
            MatchResult match = new MatchResult();
            return match;

        }
        private readonly Security _security;
    }
}