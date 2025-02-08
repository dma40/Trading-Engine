using TradingServer.Instrument;
using TradingServer.Orders;

namespace TradingServer.Orderbook
{
    public class LimitOrderbook: Orderbook, IMatchingOrderbook
    {
        // This orderbook will make use of the limits defined
        // Maybe do more work so that we can support multiple order types
        public LimitOrderbook(Security security): base(security)
        {
            _security = security;
        }

        public MatchResult match()
        {
            MatchResult result = new MatchResult();
            //List<OrderbookEntry> asks = getAskOrders();
            //List<OrderbookEntry> bids = getBidOrders();
            //while (true)
            //{
            //
            //}
            return result;
            // What should be returned in the result?
            // Maybe return a OrderRecord of all matched orders
            // Pool all the bid and ask orders together, and then match them
        }

        private readonly Security _security;
    }
}