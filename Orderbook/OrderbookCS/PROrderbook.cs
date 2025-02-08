using TradingServer.Orders;
using TradingServer.Instrument;

namespace TradingServer.Orderbook
{
    public class PROrderbook: Orderbook, IMatchingOrderbook
    {
        public PROrderbook(Security security): base(security)
        {
            _instrument = security;
        }
        public MatchResult match()
        {
            MatchResult result = new MatchResult();
            List<OrderbookEntry> asks = getAskOrders();
            List<OrderbookEntry> bids = getBidOrders();
            //while (true)
            //{
            //
            //}
            return result;
            // What should be returned in the result?
            // Maybe return a OrderRecord of all matched orders
            // Pool all the bid and ask orders together, and then match them
        }

        private readonly Security _instrument;
    }
}